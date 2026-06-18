using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.Services;

/// <summary>
/// Triển khai service cho logic nghiệp vụ Zone.
/// Xử lý CRUD và kiểm tra quy tắc nghiệp vụ cho các zone.
/// </summary>
public class ZoneService : IZoneService
{
    private readonly IZoneRepository _zoneRepository;
    private readonly IRepository<Floor> _floorRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IParkingSlotRepository _slotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ZoneService(
        IZoneRepository zoneRepository,
        IRepository<Floor> floorRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IParkingSlotRepository slotRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _floorRepository = floorRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _slotRepository = slotRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Tạo zone mới kèm xác thực.
    /// Kiểm tra tồn tại của floor và loại xe.
    /// Tự động tạo Slot nếu là loại xe ô tô.
    /// </summary>
    public async Task<ZoneDto> CreateZoneAsync(ZoneCreateRequest request)
    {
        // Kiểm tra floor tồn tại
        var floor = await _floorRepository.GetByIdAsync(request.FloorId);
        if (floor == null)
        {
            throw new NotFoundException("Floor", request.FloorId);
        }

        // Kiểm tra mã zone đã tồn tại trong floor chưa (SRS §8.3.3.7)
        var codeExists = await _zoneRepository.ZoneCodeExistsInFloorAsync(request.Code, request.FloorId);
        if (codeExists)
        {
            throw new ValidationException($"A zone with code '{request.Code}' already exists in this floor.");
        }

        // Kiểm tra loại xe tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // Bắt đầu transaction để đảm bảo tạo Zone và Slots đồng thời
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var zone = new Zone
            {
                FloorId = request.FloorId,
                Code = NormalizeZoneCode(request.Code, request.Name),
                Name = request.Name,
                Capacity = request.Capacity,
                VehicleTypeId = request.VehicleTypeId,
                AccessType = request.AccessType,
                Status = ZoneStatus.Available
            };

            await _zoneRepository.AddAsync(zone);

            await _unitOfWork.CommitAsync();

            // Map sang DTO
            return _mapper.Map<ZoneDto>(zone);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Lấy zone theo ID.
    /// </summary>
    public async Task<ZoneDto> GetZoneByIdAsync(int id)
    {
        var zone = await _zoneRepository.GetZoneWithDetailsAsync(id);
        if (zone == null)
        {
            throw new NotFoundException("Zone", id);
        }

        return _mapper.Map<ZoneDto>(zone);
    }

    /// <summary>
    /// Lấy tất cả zone.
    /// </summary>
    public async Task<IEnumerable<ZoneDto>> GetAllZonesAsync()
    {
        var zones = await _zoneRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ZoneDto>>(zones);
    }

    /// <summary>
    /// Lấy tất cả zone theo floor.
    /// </summary>
    public async Task<IEnumerable<ZoneDto>> GetZonesByFloorAsync(int floorId)
    {
        // Kiểm tra floor tồn tại
        var floor = await _floorRepository.GetByIdAsync(floorId);
        if (floor == null)
        {
            throw new NotFoundException("Floor", floorId);
        }

        var zones = await _zoneRepository.GetZonesByFloorIdAsync(floorId);
        return _mapper.Map<IEnumerable<ZoneDto>>(zones);
    }

    /// <summary>
    /// Lấy zone theo phân trang.
    /// </summary>
    public async Task<PagedResult<ZoneDto>> GetZonesPagedAsync(int pageIndex, int pageSize)
    {
        var zones = await _zoneRepository.GetAllAsync();
        var totalCount = zones.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedZones = zones
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var zoneDtos = _mapper.Map<IEnumerable<ZoneDto>>(pagedZones);

        return new PagedResult<ZoneDto>
        {
            Items = zoneDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Cập nhật zone đang tồn tại.
    /// </summary>
    public async Task<ZoneDto> UpdateZoneAsync(int id, ZoneUpdateRequest request)
    {
        // Lấy zone hiện tại
        var zone = await _zoneRepository.GetByIdAsync(id);
        if (zone == null)
        {
            throw new NotFoundException("Zone", id);
        }

        // Kiểm tra loại xe tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // Kiểm tra mã mới có trùng trong cùng floor không
        if (zone.Code != request.Code)
        {
            var codeExists = await _zoneRepository.ZoneCodeExistsInFloorAsync(request.Code, zone.FloorId);
            if (codeExists)
            {
                throw new ValidationException($"A zone with code '{request.Code}' already exists in this floor.");
            }
        }

        // Cập nhật thuộc tính zone
        zone.Code = NormalizeZoneCode(request.Code, zone.Code ?? request.Name);
        zone.Name = request.Name;
        zone.Capacity = request.Capacity;
        zone.VehicleTypeId = request.VehicleTypeId;
        zone.AccessType = request.AccessType;

        _zoneRepository.Update(zone);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ZoneDto>(zone);
    }

    private static string NormalizeZoneCode(string? code, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(code) ? fallback : code;
        return value.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Xóa zone.
    /// </summary>
    public async Task DeleteZoneAsync(int id)
    {
        var zone = await _zoneRepository.GetZoneWithDetailsAsync(id);
        if (zone == null)
        {
            throw new NotFoundException("Zone", id);
        }

        // Kiểm tra zone có chỗ đậu không
        if (zone.ParkingSlots.Any())
        {
            throw new ValidationException("Cannot delete zone that contains parking slots.");
        }

        await _zoneRepository.RemoveAsync(zone);
        await _unitOfWork.SaveChangesAsync();
    }
}

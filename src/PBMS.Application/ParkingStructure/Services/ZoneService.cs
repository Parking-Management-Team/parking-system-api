using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Entities;

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
    private readonly IMapper _mapper;

    public ZoneService(
        IZoneRepository zoneRepository,
        IRepository<Floor> floorRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _floorRepository = floorRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Tạo zone mới kèm xác thực.
    /// Kiểm tra tồn tại của floor và loại xe.
    /// </summary>
    public async Task<ZoneDto> CreateZoneAsync(ZoneCreateRequest request)
    {
        // Kiểm tra floor tồn tại
        var floor = await _floorRepository.GetByIdAsync(request.FloorId);
        if (floor == null)
        {
            throw new NotFoundException("Floor", request.FloorId);
        }

        // Kiểm tra tên zone đã tồn tại trong floor chưa
        var nameExists = await _zoneRepository.ZoneNameExistsInFloorAsync(request.Name, request.FloorId);
        if (nameExists)
        {
            throw new ValidationException($"A zone with name '{request.Name}' already exists in this floor.");
        }

        // Kiểm tra loại xe tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // Tạo entity zone mới
        var zone = new Zone
        {
            FloorId = request.FloorId,
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name,
            Capacity = request.Capacity,
            VehicleTypeId = request.VehicleTypeId,
            ZoneAccessType = request.ZoneAccessType.Trim().ToUpper(),
            Status = Domain.Enums.ZoneStatus.Available
        };

        // Thêm vào repository
        await _zoneRepository.AddAsync(zone);

        // Map sang DTO
        return _mapper.Map<ZoneDto>(zone);
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

        // Kiểm tra tên mới có trùng trong cùng floor không
        if (zone.Name != request.Name)
        {
            var nameExists = await _zoneRepository.ZoneNameExistsInFloorAsync(request.Name, zone.FloorId);
            if (nameExists)
            {
                throw new ValidationException($"A zone with name '{request.Name}' already exists in this floor.");
            }
        }

        // Cập nhật thuộc tính zone
        zone.Code = request.Code.Trim().ToUpper();
        zone.Name = request.Name;
        zone.Capacity = request.Capacity;
        zone.VehicleTypeId = request.VehicleTypeId;
        zone.ZoneAccessType = request.ZoneAccessType.Trim().ToUpper();

        _zoneRepository.Update(zone);

        return _mapper.Map<ZoneDto>(zone);
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
    }
}

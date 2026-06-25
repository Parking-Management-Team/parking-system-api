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
    /// Automatically creates slots for car zones.
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

            if (IsCarVehicleType(vehicleType))
            {
                for (var slotNumber = 1; slotNumber <= request.Capacity; slotNumber++)
                {
                    var slotCode = $"{zone.Code}-{slotNumber:D2}";
                    await _slotRepository.AddAsync(new ParkingSlot
                    {
                        Zone = zone,
                        VehicleTypeId = request.VehicleTypeId,
                        Code = slotCode,
                        Name = $"Slot {slotCode}",
                        Status = SlotStatus.Available
                    });
                }
            }

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

        // Kiểm tra loại xe mới tồn tại
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            throw new NotFoundException("VehicleType", request.VehicleTypeId);
        }

        // Lấy loại xe cũ
        var oldVehicleType = await _vehicleTypeRepository.GetByIdAsync(zone.VehicleTypeId);
        if (oldVehicleType == null)
        {
            throw new NotFoundException("VehicleType", zone.VehicleTypeId);
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

        var isOldCar = IsCarVehicleType(oldVehicleType);
        var isNewCar = IsCarVehicleType(vehicleType);
        var newZoneCode = NormalizeZoneCode(request.Code, zone.Code ?? request.Name);

        // Bắt đầu transaction để đảm bảo an toàn dữ liệu
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var currentSlots = (await _slotRepository.FindAsync(s => s.ZoneId == id)).ToList();
            var currentSlotCount = currentSlots.Count;

            if (!isOldCar && isNewCar)
            {
                // Từ Xe máy sang Ô tô: Tự động sinh slot mới tương ứng sức chứa
                for (var slotNumber = 1; slotNumber <= request.Capacity; slotNumber++)
                {
                    var slotCode = $"{newZoneCode}-{slotNumber:D2}";
                    await _slotRepository.AddAsync(new ParkingSlot
                    {
                        ZoneId = id,
                        VehicleTypeId = request.VehicleTypeId,
                        Code = slotCode,
                        Name = $"Slot {slotCode}",
                        Status = SlotStatus.Available
                    });
                }
            }
            else if (isOldCar && !isNewCar)
            {
                // Từ Ô tô sang Xe máy: Tự động xóa hết slot (nếu không bận)
                foreach (var slot in currentSlots)
                {
                    if (slot.Status == SlotStatus.Occupied)
                    {
                        throw new ValidationException($"Cannot change zone type because slot '{slot.Code}' is currently occupied.");
                    }
                    var slotWithDetails = await _slotRepository.GetSlotWithDetailsAsync(slot.Id);
                    if (slotWithDetails != null && slotWithDetails.ParkingSessions.Any(ps => string.Equals(ps.SessionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ValidationException($"Cannot change zone type because slot '{slot.Code}' has an active parking session.");
                    }
                }

                foreach (var slot in currentSlots)
                {
                    await _slotRepository.RemoveAsync(slot);
                }
            }
            else if (isOldCar && isNewCar)
            {
                // Từ Ô tô sang Ô tô: Điều chỉnh số lượng slot và đổi VehicleTypeId/Code
                if (request.Capacity > currentSlotCount)
                {
                    // Tăng sức chứa: Cập nhật slot cũ và sinh thêm slot mới
                    foreach (var slot in currentSlots)
                    {
                        slot.VehicleTypeId = request.VehicleTypeId;
                        if (zone.Code != newZoneCode)
                        {
                            var parts = slot.Code.Split('-');
                            var suffix = parts.Length > 1 ? parts[^1] : slot.Id.ToString();
                            slot.Code = $"{newZoneCode}-{suffix}";
                        }
                        _slotRepository.Update(slot);
                    }

                    for (var slotNumber = currentSlotCount + 1; slotNumber <= request.Capacity; slotNumber++)
                    {
                        var slotCode = $"{newZoneCode}-{slotNumber:D2}";
                        await _slotRepository.AddAsync(new ParkingSlot
                        {
                            ZoneId = id,
                            VehicleTypeId = request.VehicleTypeId,
                            Code = slotCode,
                            Name = $"Slot {slotCode}",
                            Status = SlotStatus.Available
                        });
                    }
                }
                else if (request.Capacity < currentSlotCount)
                {
                    // Giảm sức chứa: Xóa bớt slot từ số hiệu cao nhất (phải rảnh)
                    var slotsToRemove = currentSlots.OrderByDescending(s => s.Code).Take(currentSlotCount - request.Capacity).ToList();
                    foreach (var slot in slotsToRemove)
                    {
                        if (slot.Status == SlotStatus.Occupied)
                        {
                            throw new ValidationException($"Cannot reduce capacity because slot '{slot.Code}' is currently occupied.");
                        }
                        var slotWithDetails = await _slotRepository.GetSlotWithDetailsAsync(slot.Id);
                        if (slotWithDetails != null && slotWithDetails.ParkingSessions.Any(ps => string.Equals(ps.SessionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new ValidationException($"Cannot reduce capacity because slot '{slot.Code}' has an active parking session.");
                        }
                    }

                    foreach (var slot in slotsToRemove)
                    {
                        await _slotRepository.RemoveAsync(slot);
                    }

                    var remainingSlots = currentSlots.Except(slotsToRemove);
                    foreach (var slot in remainingSlots)
                    {
                        slot.VehicleTypeId = request.VehicleTypeId;
                        if (zone.Code != newZoneCode)
                        {
                            var parts = slot.Code.Split('-');
                            var suffix = parts.Length > 1 ? parts[^1] : slot.Id.ToString();
                            slot.Code = $"{newZoneCode}-{suffix}";
                        }
                        _slotRepository.Update(slot);
                    }
                }
                else
                {
                    // Giữ nguyên sức chứa: Chỉ cập nhật loại xe và đổi mã code của slot nếu zone code đổi
                    foreach (var slot in currentSlots)
                    {
                        slot.VehicleTypeId = request.VehicleTypeId;
                        if (zone.Code != newZoneCode)
                        {
                            var parts = slot.Code.Split('-');
                            var suffix = parts.Length > 1 ? parts[^1] : slot.Id.ToString();
                            slot.Code = $"{newZoneCode}-{suffix}";
                        }
                        _slotRepository.Update(slot);
                    }
                }
            }

            // Cập nhật thuộc tính zone
            zone.Code = newZoneCode;
            zone.Name = request.Name;
            zone.Capacity = request.Capacity;
            zone.VehicleTypeId = request.VehicleTypeId;
            zone.AccessType = request.AccessType;

            _zoneRepository.Update(zone);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ZoneDto>(zone);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static string NormalizeZoneCode(string? code, string fallback)
    {
        var value = string.IsNullOrWhiteSpace(code) ? fallback : code;
        return value.Trim().ToUpperInvariant();
    }

    private static bool IsCarVehicleType(VehicleType vehicleType)
    {
        if (string.IsNullOrWhiteSpace(vehicleType.TypeName))
        {
            return false;
        }
        return string.Equals(vehicleType.TypeName, VehicleType.CarTypeName, StringComparison.OrdinalIgnoreCase)
            || vehicleType.TypeName.Contains("CAR", StringComparison.OrdinalIgnoreCase)
            || vehicleType.TypeName.Contains("AUTO", StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// Lấy sức chứa của zone (tổng số slot và số slot đã chiếm) bất đồng bộ.
    /// </summary>
    public async Task<CapacityDto> GetZoneCapacityAsync(int id)
    {
        var zone = await _zoneRepository.GetByIdAsync(id);
        if (zone == null)
        {
            throw new NotFoundException("Zone", id);
        }

        int occupiedSlots = await _slotRepository.CountAsync(s => s.ZoneId == id && s.Status == SlotStatus.Occupied);

        return new CapacityDto
        {
            TotalSlots = zone.Capacity,
            OccupiedSlots = occupiedSlots
        };
    }
}

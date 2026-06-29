using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using BookingEntity = PBMS.Domain.Entities.Booking;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;

namespace PBMS.Application.ParkingStructure.Services;

/// <summary>
/// Triển khai service cho logic nghiệp vụ Building.
/// </summary>
public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IRepository<Floor> _floorRepository;
    private readonly IZoneRepository _zoneRepository;
    private readonly IParkingSlotRepository _slotRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<ParkingSessionEntity> _sessionRepository;
    private readonly IMapper _mapper;

    public BuildingService(
        IBuildingRepository buildingRepository, 
        IRepository<Floor> floorRepository,
        IZoneRepository zoneRepository,
        IParkingSlotRepository slotRepository,
        IRepository<VehicleType> vehicleTypeRepository,
        IBookingRepository bookingRepository,
        IRepository<ParkingSessionEntity> sessionRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _floorRepository = floorRepository;
        _zoneRepository = zoneRepository;
        _slotRepository = slotRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _bookingRepository = bookingRepository;
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<BuildingDto> CreateBuildingAsync(BuildingCreateRequest request)
    {
        // 1. Kiểm tra Building Code duy nhất
        var codeExists = await _buildingRepository.BuildingCodeExistsAsync(request.Code);
        if (codeExists)
        {
            throw new ValidationException($"Building code '{request.Code}' already exists.");
        }

        // 2. Tạo entity Building
        var building = new Building
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name,
            Address = request.Address,
            TotalFloor = request.TotalFloor,
            Status = BuildingStatus.Active
        };

        // 3. Tự động tạo các Floor tương ứng
        // Ví dụ: TotalFloor = 3 -> Tạo Tầng 1, Tầng 2, Tầng 3
        for (int i = 1; i <= request.TotalFloor; i++)
        {
            building.Floors.Add(new Floor
            {
                FloorNumber = i,
                Name = $"Floor {i}",
                Status = FloorStatus.Active
            });
        }

        await _buildingRepository.AddAsync(building);
        await _buildingRepository.SaveChangesAsync();

        return _mapper.Map<BuildingDto>(building);
    }

    public async Task<BuildingDto> GetBuildingByIdAsync(int id)
    {
        var building = await _buildingRepository.GetByIdAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        return _mapper.Map<BuildingDto>(building);
    }

    public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync()
    {
        var buildings = await _buildingRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BuildingDto>>(buildings);
    }

    public async Task<PagedResult<BuildingDto>> GetBuildingsPagedAsync(int pageIndex, int pageSize)
    {
        var buildings = await _buildingRepository.GetAllAsync();
        var totalCount = buildings.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedBuildings = buildings
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var buildingDtos = _mapper.Map<IEnumerable<BuildingDto>>(pagedBuildings);

        return new PagedResult<BuildingDto>
        {
            Items = buildingDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<BuildingDto> UpdateBuildingAsync(int id, BuildingUpdateRequest request)
    {
        var building = await _buildingRepository.GetBuildingWithDetailsAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        // Kiểm tra Building Code mới nếu thay đổi
        var newCode = request.Code.Trim().ToUpper();
        if (building.Code != newCode)
        {
            var codeExists = await _buildingRepository.BuildingCodeExistsAsync(newCode);
            if (codeExists)
            {
                throw new ValidationException($"Building code '{newCode}' already exists.");
            }
        }

        // Không cho phép giảm số tầng xuống dưới số lượng tầng hiện có trong DB
        if (request.TotalFloor < building.Floors.Count)
        {
            throw new ValidationException($"Cannot decrease total floors below the currently registered floor count ({building.Floors.Count}).");
        }

        building.Code = newCode;
        building.Name = request.Name;
        building.Address = request.Address;
        building.TotalFloor = request.TotalFloor;
        building.Status = request.Status;

        _buildingRepository.Update(building);
        await _buildingRepository.SaveChangesAsync();
        return _mapper.Map<BuildingDto>(building);
    }

    public async Task DeleteBuildingAsync(int id)
    {
        var building = await _buildingRepository.GetBuildingWithDetailsAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        // Logic bảo vệ: Không cho xóa nếu tòa nhà vẫn còn tầng (Floor)
        if (building.Floors.Any())
        {
            throw new ValidationException($"Cannot delete building '{building.Code}' because it contains floors.");
        }

        await _buildingRepository.RemoveAsync(building);
        await _buildingRepository.SaveChangesAsync();
    }

    public async Task<CapacityDto> GetBuildingCapacityAsync(int id)
    {
        var building = await _buildingRepository.GetByIdAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        var floors = await _floorRepository.FindAsync(f => f.BuildingId == id);
        var floorIds = floors.Select(f => f.Id).ToList();

        if (!floorIds.Any())
        {
            return new CapacityDto { TotalSlots = 0, OccupiedSlots = 0 };
        }

        var zones = await _zoneRepository.FindAsync(z => floorIds.Contains(z.FloorId));
        var zoneIds = zones.Select(z => z.Id).ToList();

        int totalSlots = zones.Sum(z => z.Capacity);
        int occupiedSlots = 0;

        if (zoneIds.Any())
        {
            occupiedSlots = await _slotRepository.CountAsync(s => zoneIds.Contains(s.ZoneId) && s.Status == SlotStatus.Occupied);
        }

        return new CapacityDto
        {
            TotalSlots = totalSlots,
            OccupiedSlots = occupiedSlots
        };
    }

    public async Task<BuildingAvailableCapacityDto> GetAvailableCapacityByTimeframeAsync(
        int buildingId, 
        DateTime plannedCheckinTime, 
        DateTime? plannedCheckoutTime)
    {
        var building = await _buildingRepository.GetByIdAsync(buildingId);
        if (building == null)
        {
            throw new NotFoundException("Building", buildingId);
        }

        var start = plannedCheckinTime.Kind == DateTimeKind.Utc ? plannedCheckinTime : plannedCheckinTime.ToUniversalTime();
        // Mặc định 4 tiếng nếu không truyền checkout
        var end = plannedCheckoutTime.HasValue 
            ? (plannedCheckoutTime.Value.Kind == DateTimeKind.Utc ? plannedCheckoutTime.Value : plannedCheckoutTime.Value.ToUniversalTime())
            : start.AddHours(4);

        if (end <= start)
        {
            throw new ValidationException("Planned checkout time must be greater than planned checkin time.");
        }

        var vehicleTypes = await _vehicleTypeRepository.GetAllAsync();
        var result = new BuildingAvailableCapacityDto
        {
            BuildingId = buildingId
        };

        foreach (var vt in vehicleTypes)
        {
            // 1. Tổng chỗ General = Tổng capacity Zone General của loại xe trong tòa nhà
            var totalCapacity = await _buildingRepository.GetTotalGeneralCapacityAsync(buildingId, vt.Id);

            // 2. Tính toán Sức chứa hiệu dụng sau khi trừ đi chỗ đỗ dự phòng (Buffer Slots)
            var bufferSlots = (int)Math.Ceiling(totalCapacity * (vt.BufferRatio / 100.0));
            var effectiveCapacity = totalCapacity - bufferSlots;

            // 3. Đã sử dụng = Active ParkingSession + Active Booking (Pending | Confirmed)
            var activeSessions = await _sessionRepository.CountAsync(s =>
                s.BuildingId == buildingId &&
                s.Vehicle.VehicleTypeId == vt.Id &&
                s.SessionStatus == SessionStatus.Active);

            var activeBookings = await _bookingRepository.GetActiveBookingsCountAsync(
                buildingId, vt.Id, start, end);

            var usedCapacity = activeSessions + activeBookings;
            var availableCapacity = Math.Max(0, effectiveCapacity - usedCapacity);

            result.VehicleTypeCapacities.Add(new VehicleTypeCapacityDetailDto
            {
                VehicleTypeId = vt.Id,
                VehicleTypeName = vt.TypeName,
                TotalCapacity = totalCapacity,
                BufferSlots = bufferSlots,
                EffectiveCapacity = effectiveCapacity,
                ActiveSessions = activeSessions,
                ReservedBookings = activeBookings,
                AvailableCapacity = availableCapacity
            });
        }

        return result;
    }
}

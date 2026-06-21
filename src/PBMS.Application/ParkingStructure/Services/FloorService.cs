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
/// Triển khai service cho logic nghiệp vụ Floor.
/// </summary>
public class FloorService : IFloorService
{
    private readonly IFloorRepository _floorRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IZoneRepository _zoneRepository;
    private readonly IParkingSlotRepository _slotRepository;
    private readonly IMapper _mapper;

    public FloorService(
        IFloorRepository floorRepository,
        IBuildingRepository buildingRepository,
        IZoneRepository zoneRepository,
        IParkingSlotRepository slotRepository,
        IMapper mapper)
    {
        _floorRepository = floorRepository;
        _buildingRepository = buildingRepository;
        _zoneRepository = zoneRepository;
        _slotRepository = slotRepository;
        _mapper = mapper;
    }

    public async Task<FloorDto> CreateFloorAsync(FloorCreateRequest request)
    {
        // 1. Kiểm tra building tồn tại
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building == null)
        {
            throw new NotFoundException("Building", request.BuildingId);
        }

        // 2. Kiểm tra số tầng đã tồn tại trong building chưa
        var floorExists = await _floorRepository.FloorNumberExistsInBuildingAsync(request.FloorNumber, request.BuildingId);
        if (floorExists)
        {
            throw new ValidationException($"Floor number {request.FloorNumber} already exists in this building.");
        }

        // 3. Tạo entity Floor
        var floor = new Floor
        {
            BuildingId = request.BuildingId,
            FloorNumber = request.FloorNumber,
            Name = request.Name,
            Status = Domain.Enums.FloorStatus.Active
        };

        await _floorRepository.AddAsync(floor);

        // 4. Đồng bộ số lượng tầng của Building
        building.TotalFloor++;
        _buildingRepository.Update(building);

        await _floorRepository.SaveChangesAsync();

        return _mapper.Map<FloorDto>(floor);
    }

    public async Task<FloorDto> GetFloorByIdAsync(int id)
    {
        var floor = await _floorRepository.GetByIdAsync(id);
        if (floor == null)
        {
            throw new NotFoundException("Floor", id);
        }

        return _mapper.Map<FloorDto>(floor);
    }

    public async Task<IEnumerable<FloorDto>> GetAllFloorsAsync()
    {
        var floors = await _floorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<FloorDto>>(floors);
    }

    public async Task<IEnumerable<FloorDto>> GetFloorsByBuildingAsync(int buildingId)
    {
        var building = await _buildingRepository.GetByIdAsync(buildingId);
        if (building == null)
        {
            throw new NotFoundException("Building", buildingId);
        }

        var floors = await _floorRepository.GetFloorsByBuildingIdAsync(buildingId);
        return _mapper.Map<IEnumerable<FloorDto>>(floors);
    }

    public async Task<PagedResult<FloorDto>> GetFloorsPagedAsync(int pageIndex, int pageSize)
    {
        var floors = await _floorRepository.GetAllAsync();
        var totalCount = floors.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedFloors = floors
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var floorDtos = _mapper.Map<IEnumerable<FloorDto>>(pagedFloors);

        return new PagedResult<FloorDto>
        {
            Items = floorDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<FloorDto> UpdateFloorAsync(int id, FloorUpdateRequest request)
    {
        var floor = await _floorRepository.GetByIdAsync(id);
        if (floor == null)
        {
            throw new NotFoundException("Floor", id);
        }

        // Nếu thay đổi số tầng, kiểm tra xem số mới đã tồn tại chưa
        if (floor.FloorNumber != request.FloorNumber)
        {
            var floorExists = await _floorRepository.FloorNumberExistsInBuildingAsync(request.FloorNumber, floor.BuildingId);
            if (floorExists)
            {
                throw new ValidationException($"Floor number {request.FloorNumber} already exists in this building.");
            }
        }

        floor.FloorNumber = request.FloorNumber;
        floor.Name = request.Name;
        floor.Status = request.Status;

        _floorRepository.Update(floor);
        await _floorRepository.SaveChangesAsync();
        return _mapper.Map<FloorDto>(floor);
    }

    public async Task DeleteFloorAsync(int id)
    {
        var floor = await _floorRepository.GetFloorWithDetailsAsync(id);
        if (floor == null)
        {
            throw new NotFoundException("Floor", id);
        }

        // Kiểm tra xem tầng có chứa khu vực (Zone) nào không
        if (floor.Zones.Any())
        {
            throw new ValidationException("Cannot delete floor that contains zones.");
        }

        // 1. Lấy thông tin Building để đồng bộ
        var building = await _buildingRepository.GetByIdAsync(floor.BuildingId);

        // 2. Xóa Floor
        await _floorRepository.RemoveAsync(floor);

        // 3. Giảm số lượng tầng của Building
        if (building != null)
        {
            building.TotalFloor--;
            _buildingRepository.Update(building);
        }

        await _floorRepository.SaveChangesAsync();
    }

    public async Task<CapacityDto> GetFloorCapacityAsync(int id)
    {
        var floor = await _floorRepository.GetByIdAsync(id);
        if (floor == null)
        {
            throw new NotFoundException("Floor", id);
        }

        var zones = await _zoneRepository.FindAsync(z => z.FloorId == id);
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
}

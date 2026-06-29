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
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IRepository<VehicleType> _vehicleTypeRepository;
    private readonly IMapper _mapper;

    public FloorService(
        IFloorRepository floorRepository,
        IBuildingRepository buildingRepository,
        IZoneRepository zoneRepository,
        IParkingSlotRepository slotRepository,
        IMapper mapper,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository = null!,
        IRepository<VehicleType> vehicleTypeRepository = null!)
    {
        _floorRepository = floorRepository;
        _buildingRepository = buildingRepository;
        _zoneRepository = zoneRepository;
        _slotRepository = slotRepository;
        _mapper = mapper;
        _sessionRepository = sessionRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
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

    public async Task<IEnumerable<FloorSlotSummaryDto>> GetFloorsSlotSummaryAsync(int buildingId, int? vehicleTypeId, string? status)
    {
        // 1. Kiểm tra building tồn tại
        var building = await _buildingRepository.GetByIdAsync(buildingId);
        if (building == null)
        {
            throw new NotFoundException("Building", buildingId);
        }

        // 2. Lấy danh sách các tầng của building
        var floors = await _floorRepository.GetFloorsByBuildingIdAsync(buildingId);
        var result = new List<FloorSlotSummaryDto>();

        foreach (var floor in floors)
        {
            // Lấy các zone của tầng này
            var zones = await _zoneRepository.FindAsync(z => z.FloorId == floor.Id);
            
            // Nếu có lọc theo loại xe
            if (vehicleTypeId.HasValue)
            {
                zones = zones.Where(z => z.VehicleTypeId == vehicleTypeId.Value).ToList();
            }

            var zoneIds = zones.Select(z => z.Id).ToList();
            var floorSummary = new FloorSlotSummaryDto
            {
                FloorId = floor.Id,
                FloorNumber = floor.FloorNumber,
                TotalSlots = 0,
                VehicleTypeSummaries = new List<VehicleTypeSlotSummaryDto>()
            };

            if (!zoneIds.Any())
            {
                result.Add(floorSummary);
                continue;
            }

            // Lấy tất cả slot thuộc các zone này
            var slots = await _slotRepository.FindAsync(s => zoneIds.Contains(s.ZoneId));
            
            // Lấy tất cả session active thuộc các zone này
            var activeSessions = await _sessionRepository.FindAsync(ps => 
                ps.ZoneId.HasValue && zoneIds.Contains(ps.ZoneId.Value) && 
                ps.SessionStatus.ToUpper() == "ACTIVE");

            // Gom nhóm các zone theo VehicleType để thống kê
            var zonesByVehicleType = zones.GroupBy(z => z.VehicleTypeId);

            foreach (var group in zonesByVehicleType)
            {
                var vTypeId = group.Key;
                var zonesInGroup = group.ToList();
                var zonesInGroupIds = zonesInGroup.Select(z => z.Id).ToList();

                // Lấy VehicleType details từ DB/zone
                var firstZone = zonesInGroup.First();
                string vehicleTypeName = "Unknown";
                
                if (firstZone.VehicleType != null)
                {
                    vehicleTypeName = firstZone.VehicleType.TypeName;
                }
                else
                {
                    var vt = await _vehicleTypeRepository.GetByIdAsync(vTypeId);
                    if (vt != null)
                    {
                        vehicleTypeName = vt.TypeName;
                    }
                }

                var vTypeSummary = new VehicleTypeSlotSummaryDto
                {
                    VehicleTypeId = vTypeId,
                    VehicleTypeName = vehicleTypeName,
                    TotalSlots = 0,
                    StatusCounts = new Dictionary<string, int>()
                };

                // Lọc slots và sessions thuộc nhóm này
                var groupSlots = slots.Where(s => zonesInGroupIds.Contains(s.ZoneId)).ToList();
                var groupSessions = activeSessions.Where(ps => ps.ZoneId.HasValue && zonesInGroupIds.Contains(ps.ZoneId.Value)).ToList();

                if (groupSlots.Any())
                {
                    // Zone có slot vật lý (Car)
                    vTypeSummary.TotalSlots = groupSlots.Count;

                    // Đếm theo trạng thái
                    var statusGroups = groupSlots.GroupBy(s => s.Status);
                    foreach (var sg in statusGroups)
                    {
                        var statusStr = sg.Key.ToString();
                        
                        // Lọc trạng thái nếu có yêu cầu
                        if (status != null && !statusStr.Equals(status, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        vTypeSummary.StatusCounts[statusStr] = sg.Count();
                    }

                    // Nếu lọc trạng thái mà không có slot nào khớp
                    if (status != null && !vTypeSummary.StatusCounts.ContainsKey(status))
                    {
                        vTypeSummary.StatusCounts[status] = 0;
                    }

                    // Tổng số slot của nhóm xe sau khi lọc status
                    if (status != null)
                    {
                        vTypeSummary.TotalSlots = vTypeSummary.StatusCounts.GetValueOrDefault(status, 0);
                    }
                }
                else
                {
                    // Zone quản lý theo capacity chung (Motorcycle)
                    var totalCapacity = zonesInGroup.Sum(z => z.Capacity);
                    vTypeSummary.TotalSlots = totalCapacity;

                    var occupiedCount = groupSessions.Count;
                    var availableCount = Math.Max(0, totalCapacity - occupiedCount);

                    if (status == null)
                    {
                        vTypeSummary.StatusCounts["Available"] = availableCount;
                        vTypeSummary.StatusCounts["Occupied"] = occupiedCount;
                    }
                    else if (status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                    {
                        vTypeSummary.StatusCounts["Available"] = availableCount;
                        vTypeSummary.TotalSlots = availableCount;
                    }
                    else if (status.Equals("Occupied", StringComparison.OrdinalIgnoreCase))
                    {
                        vTypeSummary.StatusCounts["Occupied"] = occupiedCount;
                        vTypeSummary.TotalSlots = occupiedCount;
                    }
                    else
                    {
                        vTypeSummary.StatusCounts[status] = 0;
                        vTypeSummary.TotalSlots = 0;
                    }
                }

                floorSummary.VehicleTypeSummaries.Add(vTypeSummary);
            }

            floorSummary.TotalSlots = floorSummary.VehicleTypeSummaries.Sum(v => v.TotalSlots);
            result.Add(floorSummary);
        }

        return result;
    }
}

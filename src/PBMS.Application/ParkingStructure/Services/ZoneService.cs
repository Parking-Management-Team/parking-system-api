using AutoMapper;
using PBMS.Application.Common;
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
    public async Task<BaseResponse<ZoneDto>> CreateZoneAsync(ZoneCreateRequest request)
    {
        try
        {
            // Kiểm tra floor tồn tại
            var floor = await _floorRepository.GetByIdAsync(request.FloorId);
            if (floor == null)
            {
                return BaseResponse<ZoneDto>.Fail("FLOOR_NOT_FOUND", $"Floor with ID {request.FloorId} not found.");
            }

            // Kiểm tra tên zone đã tồn tại trong floor chưa
            var nameExists = await _zoneRepository.ZoneNameExistsInFloorAsync(request.Name, request.FloorId);
            if (nameExists)
            {
                return BaseResponse<ZoneDto>.Fail("ZONE_NAME_EXISTS", $"A zone with name '{request.Name}' already exists in this floor.");
            }

            //Kiểm tra loại xe tồn tại
            var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null)
            {
                return BaseResponse<ZoneDto>.Fail("VEHICLE_TYPE_NOT_FOUND", $"VehicleType with ID {request.VehicleTypeId} not found.");
            }

            // Tạo entity zone mới
            var zone = new Zone
            {
                FloorId = request.FloorId,
                Name = request.Name,
                Capacity = request.Capacity,
                VehicleTypeId = request.VehicleTypeId,
                Status = Domain.Enums.ZoneStatus.Available
            };

            // Thêm vào repository
            await _zoneRepository.AddAsync(zone);

            // Map sang DTO
            var zoneDto = _mapper.Map<ZoneDto>(zone);

            return BaseResponse<ZoneDto>.Ok(zoneDto, "Zone created successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<ZoneDto>.Fail("CREATE_ZONE_ERROR", $"An error occurred while creating the zone: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy zone theo ID.
    /// </summary>
    public async Task<BaseResponse<ZoneDto>> GetZoneByIdAsync(int id)
    {
        try
        {
            var zone = await _zoneRepository.GetZoneWithDetailsAsync(id);
            if (zone == null)
            {
                return BaseResponse<ZoneDto>.Fail("ZONE_NOT_FOUND", $"Zone with ID {id} not found.");
            }

            var zoneDto = _mapper.Map<ZoneDto>(zone);
            return BaseResponse<ZoneDto>.Ok(zoneDto);
        }
        catch (Exception ex)
        {
            return BaseResponse<ZoneDto>.Fail("GET_ZONE_ERROR", $"An error occurred while retrieving the zone: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy tất cả zone.
    /// </summary>
    public async Task<BaseResponse<IEnumerable<ZoneDto>>> GetAllZonesAsync()
    {
        try
        {
            var zones = await _zoneRepository.GetAllAsync();
            var zoneDtos = _mapper.Map<IEnumerable<ZoneDto>>(zones);
            return BaseResponse<IEnumerable<ZoneDto>>.Ok(zoneDtos);
        }
        catch (Exception ex)
        {
            return BaseResponse<IEnumerable<ZoneDto>>.Fail("GET_ZONES_ERROR", $"An error occurred while retrieving zones: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy tất cả zone theo floor.
    /// </summary>
    public async Task<BaseResponse<IEnumerable<ZoneDto>>> GetZonesByFloorAsync(int floorId)
    {
        try
        {
            // Kiểm tra floor tồn tại
            var floor = await _floorRepository.GetByIdAsync(floorId);
            if (floor == null)
            {
                return BaseResponse<IEnumerable<ZoneDto>>.Fail("FLOOR_NOT_FOUND", $"Floor with ID {floorId} not found.");
            }

            var zones = await _zoneRepository.GetZonesByFloorIdAsync(floorId);
            var zoneDtos = _mapper.Map<IEnumerable<ZoneDto>>(zones);
            return BaseResponse<IEnumerable<ZoneDto>>.Ok(zoneDtos);
        }
        catch (Exception ex)
        {
            return BaseResponse<IEnumerable<ZoneDto>>.Fail("GET_ZONES_ERROR", $"An error occurred while retrieving zones: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy zone theo phân trang.
    /// </summary>
    public async Task<BaseResponse<PagedResult<ZoneDto>>> GetZonesPagedAsync(int pageIndex, int pageSize)
    {
        try
        {
            var zones = await _zoneRepository.GetAllAsync();
            var totalCount = zones.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var pagedZones = zones
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var zoneDtos = _mapper.Map<IEnumerable<ZoneDto>>(pagedZones);

            var result = new PagedResult<ZoneDto>
            {
                Items = zoneDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return BaseResponse<PagedResult<ZoneDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            return BaseResponse<PagedResult<ZoneDto>>.Fail("GET_ZONES_ERROR", $"An error occurred while retrieving zones: {ex.Message}");
        }
    }

    /// <summary>
    /// Cập nhật zone đang tồn tại.
    /// </summary>
    public async Task<BaseResponse<ZoneDto>> UpdateZoneAsync(int id, ZoneUpdateRequest request)
    {
        try
        {
            // Lấy zone hiện tại
            var zone = await _zoneRepository.GetByIdAsync(id);
            if (zone == null)
            {
                return BaseResponse<ZoneDto>.Fail("ZONE_NOT_FOUND", $"Zone with ID {id} not found.");
            }

            // Kiểm tra loại xe tồn tại
            var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null)
            {
                return BaseResponse<ZoneDto>.Fail("VEHICLE_TYPE_NOT_FOUND", $"VehicleType with ID {request.VehicleTypeId} not found.");
            }

            // Kiểm tra tên mới có trùng trong cùng floor không
            if (zone.Name != request.Name)
            {
                var nameExists = await _zoneRepository.ZoneNameExistsInFloorAsync(request.Name, zone.FloorId);
                if (nameExists)
                {
                    return BaseResponse<ZoneDto>.Fail("ZONE_NAME_EXISTS", $"A zone with name '{request.Name}' already exists in this floor.");
                }
            }

            // Cập nhật thuộc tính zone
            zone.Name = request.Name;
            zone.Capacity = request.Capacity;
            zone.VehicleTypeId = request.VehicleTypeId;

            _zoneRepository.Update(zone);

            var zoneDto = _mapper.Map<ZoneDto>(zone);
            return BaseResponse<ZoneDto>.Ok(zoneDto, "Zone updated successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<ZoneDto>.Fail("UPDATE_ZONE_ERROR", $"An error occurred while updating the zone: {ex.Message}");
        }
    }

    /// <summary>
    /// Xóa zone.
    /// </summary>
    public async Task<BaseResponse<string>> DeleteZoneAsync(int id)
    {
        try
        {
            var zone = await _zoneRepository.GetByIdAsync(id);
            if (zone == null)
            {
                return BaseResponse<string>.Fail("ZONE_NOT_FOUND", $"Zone with ID {id} not found.");
            }

            // Kiểm tra zone có chỗ đậu không
            var hasParkingSlots = zone.ParkingSlots.Any();
            if (hasParkingSlots)
            {
                return BaseResponse<string>.Fail("ZONE_HAS_SLOTS", "Cannot delete zone that contains parking slots.");
            }

            await _zoneRepository.RemoveAsync(zone);
            return BaseResponse<string>.Ok(id.ToString(), "Zone deleted successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<string>.Fail("DELETE_ZONE_ERROR", $"An error occurred while deleting the zone: {ex.Message}");
        }
    }
}

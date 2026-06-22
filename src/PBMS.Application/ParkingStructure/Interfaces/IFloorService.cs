using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;

namespace PBMS.Application.ParkingStructure.Interfaces;

/// <summary>
/// Interface service cho nghiệp vụ Floor.
/// </summary>
public interface IFloorService
{
    Task<FloorDto> CreateFloorAsync(FloorCreateRequest request);
    Task<FloorDto> GetFloorByIdAsync(int id);
    Task<IEnumerable<FloorDto>> GetAllFloorsAsync();
    Task<IEnumerable<FloorDto>> GetFloorsByBuildingAsync(int buildingId);
    Task<PagedResult<FloorDto>> GetFloorsPagedAsync(int pageIndex, int pageSize);
    Task<FloorDto> UpdateFloorAsync(int id, FloorUpdateRequest request);
    Task DeleteFloorAsync(int id);
    Task<CapacityDto> GetFloorCapacityAsync(int id);
    Task<IEnumerable<FloorSlotSummaryDto>> GetFloorsSlotSummaryAsync(int buildingId, int? vehicleTypeId, string? status);
}

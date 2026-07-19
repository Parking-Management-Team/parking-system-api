using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;

namespace PBMS.Application.ParkingStructure.Interfaces;

/// <summary>
/// Interface service cho nghiệp vụ Building.
/// </summary>
public interface IBuildingService
{
    Task<BuildingDto> CreateBuildingAsync(BuildingCreateRequest request);
    Task<BuildingDto> GetBuildingByIdAsync(int id);
    Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync();
    Task<PagedResult<BuildingDto>> GetBuildingsPagedAsync(int pageIndex, int pageSize);
    Task<BuildingDto> UpdateBuildingAsync(int id, BuildingUpdateRequest request);
    Task DeleteBuildingAsync(int id);
    Task<CapacityDto> GetBuildingCapacityAsync(int id);
    Task<BuildingAvailableCapacityDto> GetAvailableCapacityByTimeframeAsync(int buildingId, DateTime plannedCheckinTime, DateTime? plannedCheckoutTime);
}

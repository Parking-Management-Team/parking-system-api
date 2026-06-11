using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;

namespace PBMS.Application.ParkingStructure.Interfaces;

/// <summary>
/// Interface service cho nghiệp vụ ParkingSlot.
/// </summary>
public interface IParkingSlotService
{
    Task<ParkingSlotDto> CreateSlotAsync(ParkingSlotCreateRequest request);
    Task<ParkingSlotDto> GetSlotByIdAsync(int id);
    Task<IEnumerable<ParkingSlotDto>> GetAllSlotsAsync();
    Task<IEnumerable<ParkingSlotDto>> GetSlotsByZoneAsync(int zoneId);
    Task<PagedResult<ParkingSlotDto>> GetSlotsPagedAsync(int pageIndex, int pageSize);
    Task<ParkingSlotDto> UpdateSlotAsync(int id, ParkingSlotUpdateRequest request);
    Task DeleteSlotAsync(int id);
}

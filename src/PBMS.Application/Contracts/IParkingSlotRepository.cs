using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho entity ParkingSlot.
/// </summary>
public interface IParkingSlotRepository : IRepository<ParkingSlot>
{
    /// <summary>
    /// Lấy tất cả slot thuộc một khu vực (Zone) bất đồng bộ.
    /// </summary>
    Task<IEnumerable<ParkingSlot>> GetSlotsByZoneIdAsync(int zoneId);

    /// <summary>
    /// Kiểm tra xem mã slot đã tồn tại trong hệ thống chưa.
    /// </summary>
    Task<bool> SlotCodeExistsAsync(string slotCode);

    /// <summary>
    /// Lấy chi tiết slot kèm theo các thông tin liên quan (Zone, VehicleType).
    /// </summary>
    Task<ParkingSlot?> GetSlotWithDetailsAsync(int id);

    /// <summary>
    /// Finds an available slot in the same zone, excluding the originally booked slot
    /// and any slot that has an active session or a confirmed booking overlapping the
    /// given time window (with buffer).
    /// Used by the Auto Delay Fallback logic at check-in.
    /// </summary>
    /// <param name="zoneId">Zone to search within.</param>
    /// <param name="excludeSlotId">The originally booked slot to exclude.</param>
    /// <param name="checkinTime">Start of the booking's planned time window (UTC).</param>
    /// <param name="checkoutTime">End of the booking's planned time window (UTC).</param>
    /// <param name="bufferMinutes">Buffer minutes applied when checking booking overlaps.</param>
    Task<ParkingSlot?> FindFallbackSlotAsync(int zoneId, int excludeSlotId, DateTime checkinTime, DateTime checkoutTime, int bufferMinutes);

    /// <summary>
    /// Finds an available parking slot in a Monthly zone for the given building and vehicle type.
    /// Used for monthly subscription slot assignment.
    /// </summary>
    Task<ParkingSlot?> FindAvailableMonthlySlotAsync(int buildingId, int vehicleTypeId);
}

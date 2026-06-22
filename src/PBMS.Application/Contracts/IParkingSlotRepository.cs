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
    /// Tìm vị trí đỗ xe trống thuộc Zone có AccessType là Monthly phục vụ cho đăng ký ô tô tháng.
    /// </summary>
    Task<ParkingSlot?> FindAvailableMonthlySlotAsync(int buildingId, int vehicleTypeId);
}


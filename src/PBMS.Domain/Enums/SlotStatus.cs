namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của một vị trí đỗ xe cụ thể (Parking Slot).
/// Tham chiếu SRS: §8.3.3.9
/// </summary>
public enum SlotStatus
{
    /// <summary>
    /// Vị trí còn trống và có thể sử dụng.
    /// </summary>
    Available,

    /// <summary>
    /// Vị trí đã được đặt trước thông qua Booking.
    /// </summary>
    Reserved,

    /// <summary>
    /// Vị trí đang có xe đỗ.
    /// </summary>
    Occupied,

    /// <summary>
    /// Vị trí bị khóa do bảo trì hoặc mục đích khác.
    /// </summary>
    Blocked
}

namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của một vị trí đỗ xe cụ thể (Parking Slot).
/// Tham chiếu SRS: §8.3.3.9
/// </summary>
public enum SlotStatus
{
    /// <summary>
    /// Slot trống.
    /// </summary>
    Available,

    /// <summary>
    /// Slot có xe.
    /// </summary>
    Occupied,

    /// <summary>
    /// Slot bị khóa.
    /// </summary>
    Blocked,

    /// <summary>
    /// Slot đang bảo trì.
    /// </summary>
    Maintenance
}

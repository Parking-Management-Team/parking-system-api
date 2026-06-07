namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của một tầng (Floor) trong hệ thống.
/// </summary>
public enum FloorStatus
{
    /// <summary>
    /// Tầng đang hoạt động bình thường và có thể tiếp nhận xe.
    /// </summary>
    Available,

    /// <summary>
    /// Tầng đã đầy xe (tất cả các zone/slot bên trong đều bận).
    /// </summary>
    Occupied,

    /// <summary>
    /// Tầng đang được giữ chỗ cho một mục đích đặc biệt hoặc sự kiện.
    /// </summary>
    Reserved,

    /// <summary>
    /// Tầng đang bảo trì hoặc tạm đóng cửa, không cho phép gửi xe.
    /// </summary>
    OutOfService
}

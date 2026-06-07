namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái hoạt động của một tòa nhà (Building).
/// </summary>
public enum BuildingStatus
{
    /// <summary>
    /// Tòa nhà đang mở cửa và hoạt động bình thường.
    /// </summary>
    Available,

    /// <summary>
    /// Tòa nhà đã đầy chỗ gửi xe.
    /// </summary>
    Occupied,

    /// <summary>
    /// Tòa nhà đang được giữ chỗ cho sự kiện hoặc mục đích riêng.
    /// </summary>
    Reserved,

    /// <summary>
    /// Tòa nhà đang bảo trì hoặc tạm đóng cửa.
    /// </summary>
    OutOfService
}

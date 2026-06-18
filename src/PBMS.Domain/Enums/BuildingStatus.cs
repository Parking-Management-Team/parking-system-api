namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái hoạt động của một tòa nhà (Building).
/// </summary>
public enum BuildingStatus
{
    /// <summary>
    /// Nhà xe chưa được đưa vào sử dụng.
    /// </summary>
    Inactive,

    /// <summary>
    /// Nhà xe hoạt động, cho phép xe ra vào.
    /// </summary>
    Active,

    /// <summary>
    /// Bãi xe được bảo trì; không tiếp nhận xe mới. Cho phép xe đang gửi ra khỏi bãi.
    /// </summary>
    Maintenance,

    /// <summary>
    /// Nhà xe không thể hoạt động.
    /// </summary>
    OutOfService
}

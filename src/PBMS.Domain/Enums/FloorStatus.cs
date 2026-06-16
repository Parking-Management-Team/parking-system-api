namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của một tầng (Floor) trong hệ thống.
/// </summary>
public enum FloorStatus
{
    /// <summary>
    /// Tầng chưa được đưa vào sử dụng.
    /// </summary>
    Inactive,

    /// <summary>
    /// Tầng được sử dụng và cho phép xe sử dụng các vị trí đỗ.
    /// </summary>
    Active,

    /// <summary>
    /// Tầng được bảo trì; không tiếp nhận xe mới. Cho phép xe đang gửi ra khỏi bãi.
    /// </summary>
    Maintenance,

    /// <summary>
    /// Tầng đang gặp sự cố hoặc không thể phục vụ.
    /// </summary>
    OutOfService
}

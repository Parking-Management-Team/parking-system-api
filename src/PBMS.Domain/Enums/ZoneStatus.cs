namespace PBMS.Domain.Enums;

public enum ZoneStatus
{
    /// <summary>
    /// Khu vực trống.
    /// </summary>
    Available,

    /// <summary>
    /// Khu vực không nhận thêm xe.
    /// </summary>
    Occupied,

    /// <summary>
    /// Khu vực không hoạt động.
    /// </summary>
    Blocked
}
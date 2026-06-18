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
    Blocked,
    Maintenance,
    Reserved,
    OutOfService
}

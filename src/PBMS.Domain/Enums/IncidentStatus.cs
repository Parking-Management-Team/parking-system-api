namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của một sự cố (Incident) trong hệ thống.
/// Tham chiếu SRS: §6.13.5
/// </summary>
public enum IncidentStatus
{
    /// <summary>
    /// Sự cố mới được ghi nhận và đang chờ xử lý.
    /// </summary>
    Open,

    /// <summary>
    /// Sự cố đang được nhân viên xử lý.
    /// </summary>
    Processing,

    /// <summary>
    /// Sự cố đã được xử lý hoàn tất.
    /// </summary>
    Resolved,

    /// <summary>
    /// Sự cố đã bị hủy hoặc không tiếp tục xử lý.
    /// </summary>
    Cancelled
}

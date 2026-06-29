namespace PBMS.Application.MonthlyCard.DTOs;

/// <summary>
/// Yêu cầu lọc và phân trang danh sách đăng ký vé tháng.
/// </summary>
public class MonthlySubscriptionFilterRequest
{
    /// <summary>
    /// Số trang (bắt đầu từ 1). Mặc định: 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Số lượng bản ghi trên một trang. Mặc định: 10.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Lọc theo trạng thái (PENDING, ACTIVE, EXPIRED, DOWNGRADED, CANCELLED).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Lọc theo ID tòa nhà.
    /// </summary>
    public int? BuildingId { get; set; }

    /// <summary>
    /// Lọc theo ID tài khoản.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Tìm kiếm theo biển số xe.
    /// </summary>
    public string? LicensePlate { get; set; }

    /// <summary>
    /// Tìm kiếm theo mã thẻ.
    /// </summary>
    public string? CardCode { get; set; }
}
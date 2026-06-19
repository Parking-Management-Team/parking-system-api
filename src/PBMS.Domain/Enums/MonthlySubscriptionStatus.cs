namespace PBMS.Domain.Enums;

/// <summary>
/// Các trạng thái của hồ sơ đăng ký vé tháng (Monthly Subscription).
/// </summary>
public static class MonthlySubscriptionStatus
{
    /// <summary>
    /// Đang chờ thanh toán để kích hoạt chu kỳ mới.
    /// </summary>
    public const string Pending = "PENDING";

    /// <summary>
    /// Vé tháng đang hoạt động và còn hiệu lực.
    /// </summary>
    public const string Active = "ACTIVE";

    /// <summary>
    /// Vé tháng đã hết hạn sử dụng.
    /// </summary>
    public const string Expired = "EXPIRED";

    /// <summary>
    /// Quyền lợi thẻ tháng bị downgrade (hết hạn khi xe vẫn đang ở trong bãi).
    /// </summary>
    public const string Downgraded = "DOWNGRADED";

    /// <summary>
    /// Đăng ký tháng đã bị hủy (do quá hạn thanh toán hoặc chủ động hủy).
    /// </summary>
    public const string Cancelled = "CANCELLED";
}

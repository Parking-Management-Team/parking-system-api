namespace PBMS.Domain.Enums;

/// <summary>
/// Loại quyền truy cập/sử dụng của một khu vực đỗ xe (Zone).
/// Dùng để tách biệt khu vực dành cho xe vãng lai và xe thẻ tháng.
/// Tham chiếu SRS: §8.3.3.7
/// </summary>
public enum ZoneAccessType
{
    /// <summary>
    /// Khu vực chung dành cho xe vãng lai (Walk-in) và xe đặt chỗ (Booking).
    /// </summary>
    General,

    /// <summary>
    /// Khu vực dành riêng cho xe đăng ký thẻ tháng (Monthly Subscription).
    /// </summary>
    Monthly
}

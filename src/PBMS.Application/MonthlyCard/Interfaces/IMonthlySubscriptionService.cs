using PBMS.Application.MonthlyCard.DTOs;

namespace PBMS.Application.MonthlyCard.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ đăng ký và quản lý vé tháng.
/// </summary>
public interface IMonthlySubscriptionService
{
    /// <summary>
    /// Đăng ký vé tháng mới (Trạng thái mặc định: PENDING).
    /// </summary>
    Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateSubscriptionRequest request);

    /// <summary>
    /// Lấy thông tin đăng ký vé tháng theo ID.
    /// </summary>
    Task<MonthlySubscriptionDto> GetSubscriptionByIdAsync(int id);

    /// <summary>
    /// Hủy đăng ký vé tháng.
    /// </summary>
    Task CancelSubscriptionAsync(int id);

    /// <summary>
    /// Dọn dẹp các hồ sơ PENDING quá hạn thanh toán.
    /// </summary>
    Task CleanupExpiredPendingSubscriptionsAsync(int timeoutMinutes);
}

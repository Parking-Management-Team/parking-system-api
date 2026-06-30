using PBMS.Application.Common;
using PBMS.Application.MonthlyCard.DTOs;

namespace PBMS.Application.MonthlyCard.Interfaces;

/// <summary>
/// Service interface for monthly subscription management.
/// </summary>
public interface IMonthlySubscriptionService
{
    /// <summary>
    /// Register a new monthly subscription (default status: PENDING).
    /// </summary>
    Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateSubscriptionRequest request);

    /// <summary>
    /// Get monthly subscription by ID.
    /// </summary>
    Task<MonthlySubscriptionDto> GetSubscriptionByIdAsync(int id);

    /// <summary>
    /// Get list of monthly subscriptions with filtering and pagination.
    /// </summary>
    Task<PagedResult<MonthlySubscriptionDto>> GetAllSubscriptionsAsync(MonthlySubscriptionFilterRequest filter);

    /// <summary>
    /// Activate monthly subscription (PENDING -> ACTIVE).
    /// </summary>
    Task<MonthlySubscriptionDto> ActivateSubscriptionAsync(int id);

    /// <summary>
    /// Cancel monthly subscription.
    /// </summary>
    Task CancelSubscriptionAsync(int id);

    /// <summary>
    /// Update monthly subscription (card only).
    /// </summary>
    Task<MonthlySubscriptionDto> UpdateSubscriptionAsync(int id, UpdateSubscriptionRequest request);

    /// <summary>
    /// Cleanup expired pending subscriptions.
    /// </summary>
    Task CleanupExpiredPendingSubscriptionsAsync(int timeoutMinutes);

    /// <summary>
    /// Thay thế thẻ gửi xe mới cho vé tháng khi bị mất thẻ.
    /// </summary>
    Task<MonthlySubscriptionDto> ReplaceSubscriptionCardAsync(int subscriptionId, string newCardCode);

    /// <summary>
    /// Gia hạn đăng ký vé tháng thêm 30 ngày.
    /// </summary>
    Task<MonthlySubscriptionDto> RenewSubscriptionAsync(int id);
}

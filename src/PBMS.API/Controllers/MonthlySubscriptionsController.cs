using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Interfaces;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller quản lý Đăng ký vé tháng (Monthly Subscription).
/// </summary>
[ApiController]
[Route("api/monthly-subscriptions")]
public class MonthlySubscriptionsController : ControllerBase
{
    private readonly IMonthlySubscriptionService _subscriptionService;

    public MonthlySubscriptionsController(IMonthlySubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Đăng ký vé tháng mới (Trạng thái mặc định: PENDING).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> RegisterSubscription([FromBody] CreateSubscriptionRequest request)
    {
        var subscription = await _subscriptionService.RegisterSubscriptionAsync(request);
        return CreatedAtAction(
            nameof(GetSubscriptionById),
            new { id = subscription.Id },
            BaseResponse<MonthlySubscriptionDto>.Ok(subscription, "Monthly subscription registered successfully.")
        );
    }

    /// <summary>
    /// Lấy thông tin chi tiết đăng ký vé tháng theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> GetSubscriptionById(int id)
    {
        var subscription = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(subscription));
    }

    /// <summary>
    /// Hủy đăng ký vé tháng.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse<string>>> CancelSubscription(int id)
    {
        await _subscriptionService.CancelSubscriptionAsync(id);
        return Ok(BaseResponse<string>.Ok("Monthly subscription cancelled successfully."));
    }

    /// <summary>
    /// Dọn dẹp các hồ sơ PENDING quá hạn thanh toán.
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<BaseResponse<string>>> CleanupExpiredPendingSubscriptions([FromQuery] int timeoutMinutes = 15)
    {
        await _subscriptionService.CleanupExpiredPendingSubscriptionsAsync(timeoutMinutes);
        return Ok(BaseResponse<string>.Ok("Expired pending monthly subscriptions cleaned up successfully."));
    }
}

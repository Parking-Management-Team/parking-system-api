using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Interfaces;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller for Monthly Subscription management.
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
    /// Get list of monthly subscriptions with filtering and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<MonthlySubscriptionDto>>>> GetAll([FromQuery] MonthlySubscriptionFilterRequest filter)
    {
        var result = await _subscriptionService.GetAllSubscriptionsAsync(filter);
        return Ok(BaseResponse<PagedResult<MonthlySubscriptionDto>>.Ok(result));
    }

    /// <summary>
    /// Register a new monthly subscription (default status: PENDING).
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
    /// Get monthly subscription details by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> GetSubscriptionById(int id)
    {
        var subscription = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(subscription));
    }

    /// <summary>
    /// Update monthly subscription (card only).
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> UpdateSubscription(int id, [FromBody] UpdateSubscriptionRequest request)
    {
        var subscription = await _subscriptionService.UpdateSubscriptionAsync(id, request);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(subscription, "Monthly subscription updated successfully."));
    }

    /// <summary>
    /// Activate monthly subscription (PENDING -> ACTIVE).
    /// </summary>
    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> ActivateSubscription(int id)
    {
        var subscription = await _subscriptionService.ActivateSubscriptionAsync(id);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(subscription, "Monthly subscription activated successfully."));
    }

    /// <summary>
    /// Cancel monthly subscription.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse<string>>> CancelSubscription(int id)
    {
        await _subscriptionService.CancelSubscriptionAsync(id);
        return Ok(BaseResponse<string>.Ok("Monthly subscription cancelled successfully."));
    }

    /// <summary>
    /// Cleanup expired pending subscriptions.
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<BaseResponse<string>>> CleanupExpiredPendingSubscriptions([FromQuery] int timeoutMinutes = 15)
    {
        await _subscriptionService.CleanupExpiredPendingSubscriptionsAsync(timeoutMinutes);
        return Ok(BaseResponse<string>.Ok("Expired pending monthly subscriptions cleaned up successfully."));
    }
}

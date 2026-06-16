using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

[ApiController]
[Route("api/monthly-subscriptions")]
public class MonthlySubscriptionsController : ControllerBase
{
    private readonly IMonthlySubscriptionService _service;

    public MonthlySubscriptionsController(IMonthlySubscriptionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> Register(
        [FromBody] CreateMonthlySubscriptionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.RegisterSubscriptionAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            BaseResponse<MonthlySubscriptionDto>.Ok(result, "Đăng ký vé tháng thành công. Trạng thái hiện tại: PENDING.")
        );
    }

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> Activate(
        int id,
        [FromBody] AssignCardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.ActivateSubscriptionAsync(id, request.CardCode);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(result, "Kích hoạt vé tháng thành công. Trạng thái hiện tại: ACTIVE."));
    }

    [HttpPost("{id:int}/renew")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> Renew(
        int id,
        [FromBody] RenewSubscriptionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.RenewSubscriptionAsync(id, request.Months);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(result, $"Gia hạn vé tháng thành công thêm {request.Months} tháng."));
    }

    [HttpPost("{id:int}/lost-card")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> ReportLostCard(int id)
    {
        var result = await _service.ReportLostCardAsync(id);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(result, "Báo mất thẻ tháng thành công. Thẻ cũ đã bị đánh dấu Lost và gỡ liên kết."));
    }

    [HttpPost("{id:int}/reassign-card")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> ReassignCard(
        int id,
        [FromBody] AssignCardRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.ReassignCardAsync(id, request.CardCode);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(result, "Cấp/gán thẻ tháng mới thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<MonthlySubscriptionDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(BaseResponse<MonthlySubscriptionDto>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<IEnumerable<MonthlySubscriptionDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(BaseResponse<IEnumerable<MonthlySubscriptionDto>>.Ok(result));
    }
}

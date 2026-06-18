using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Revenue.DTOs;
using PBMS.Application.Revenue.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý thống kê doanh thu (Revenue).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenueService;

    public RevenueController(IRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    /// <summary>
    /// Lấy danh sách thống kê doanh thu theo bộ lọc (Building, Ngày bắt đầu/kết thúc, Loại chu kỳ) có phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRevenueStatistics(
        [FromQuery] RevenueFilterDto filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _revenueService.GetRevenueStatisticsAsync(filter, pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<RevenueStatisticDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy chi tiết một bản ghi thống kê doanh thu theo ID (kèm theo các giao dịch thanh toán cụ thể).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRevenueStatisticById(int id)
    {
        try
        {
            var result = await _revenueService.GetRevenueStatisticByIdAsync(id);
            return Ok(BaseResponse<RevenueStatisticDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(BaseResponse<string>.Fail("REVENUE_STATISTIC_NOT_FOUND", ex.Message));
        }
    }
}

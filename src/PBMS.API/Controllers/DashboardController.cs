using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Common.DTOs;
using PBMS.Application.Common.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// API báo cáo tổng quan dành cho Admin/Quản trị hệ thống.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// GET /api/dashboard/summary
    /// Lấy báo cáo các chỉ số thống kê tổng quan của toàn bộ hệ thống bãi đỗ xe.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse<DashboardSummaryDto>>> GetSummary()
    {
        var result = await _dashboardService.GetSummaryAsync();
        return Ok(result);
    }
}

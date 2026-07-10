using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ShiftReport.DTOs;
using PBMS.Application.ShiftReport.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý Báo cáo ca trực (Shift Reports) dành cho Staff và Manager.
/// </summary>
[ApiController]
[Route("api/shift-reports")]
public class ShiftReportsController : ControllerBase
{
    private readonly IShiftReportService _shiftReportService;

    public ShiftReportsController(IShiftReportService shiftReportService)
    {
        _shiftReportService = shiftReportService;
    }

    /// <summary>
    /// GET /api/shift-reports/preview?staffId={staffId}
    /// Xem trước số liệu ca hiện tại của nhân viên dựa theo thời gian thực tế.
    /// </summary>
    [HttpGet("preview")]
    public async Task<IActionResult> PreviewShiftReport([FromQuery] int staffId)
    {
        var result = await _shiftReportService.PreviewShiftReportAsync(staffId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// POST /api/shift-reports
    /// Nộp báo cáo ca trực chính thức lên hệ thống (Staff).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitShiftReport([FromBody] CreateShiftReportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _shiftReportService.SubmitShiftReportAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// POST /api/shift-reports/{id}/approve?managerId={managerId}&approve={approve}
    /// Phê duyệt hoặc Từ chối đối soát báo cáo ca trực (Manager).
    /// </summary>
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveShiftReport(int id, [FromQuery] int managerId, [FromQuery] bool approve)
    {
        var result = await _shiftReportService.ApproveShiftReportAsync(id, managerId, approve);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// GET /api/shift-reports
    /// Lấy danh sách các báo cáo ca trực có phân trang (Manager).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetShiftReports([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _shiftReportService.GetShiftReportsPagedAsync(pageIndex, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

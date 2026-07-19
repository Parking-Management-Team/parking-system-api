using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.AuditLog.DTOs;
using PBMS.Application.AuditLog.Interfaces;
using PBMS.Application.Common;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý Nhật ký thao tác (Audit Log).
/// Chỉ Admin và Manager được truy cập.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Lấy danh sách nhật ký thao tác có phân trang và lọc.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? accountId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? targetTable = null)
    {
        var result = await _auditLogService.GetAuditLogsAsync(pageIndex, pageSize, accountId, action, targetTable);
        return Ok(BaseResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy chi tiết một bản ghi nhật ký theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLogById(int id)
    {
        var result = await _auditLogService.GetAuditLogByIdAsync(id);
        if (result == null)
            return NotFound(BaseResponse<object>.Fail("NOT_FOUND", $"Audit log with ID {id} not found."));
        return Ok(BaseResponse<AuditLogDto>.Ok(result));
    }
}
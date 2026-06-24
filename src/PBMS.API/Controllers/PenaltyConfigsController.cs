using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý cấu hình giá phạt sự cố (PenaltyConfig).
/// </summary>
[ApiController]
[Route("api/penalty-configs")]
[Authorize]
public class PenaltyConfigsController : ControllerBase
{
    private readonly IPenaltyConfigService _service;

    public PenaltyConfigsController(IPenaltyConfigService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách cấu hình giá phạt sự cố (hỗ trợ lọc theo loại sự cố và trạng thái hoạt động).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? incidentTypeId, [FromQuery] bool? onlyActive)
    {
        var result = await _service.GetAllConfigsAsync(incidentTypeId, onlyActive);
        return Ok(BaseResponse<IEnumerable<PenaltyConfigDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy cấu hình giá phạt đang hoạt động của một loại sự cố.
    /// </summary>
    [HttpGet("active/{incidentTypeId}")]
    public async Task<IActionResult> GetActive(int incidentTypeId)
    {
        var result = await _service.GetActiveConfigByIncidentTypeAsync(incidentTypeId);
        if (result == null)
        {
            return NotFound(BaseResponse<PenaltyConfigDto>.Fail("NOT_FOUND", $"No active penalty configuration found for incident type ID: {incidentTypeId}"));
        }
        return Ok(BaseResponse<PenaltyConfigDto>.Ok(result));
    }

    /// <summary>
    /// Tạo cấu hình giá phạt mới (Tự động vô hiệu hóa giá cũ).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreatePenaltyConfigRequest request)
    {
        var result = await _service.CreateConfigAsync(request);
        return CreatedAtAction(nameof(GetActive), new { incidentTypeId = result.IncidentTypeId }, BaseResponse<PenaltyConfigDto>.Ok(result, "Penalty configuration created successfully."));
    }

    /// <summary>
    /// Vô hiệu hóa (Inactive) cấu hình giá phạt.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var success = await _service.DeactivateConfigAsync(id);
        if (!success)
        {
            return BadRequest(BaseResponse<string>.Fail("BAD_REQUEST", "Configuration is already inactive or cannot be updated."));
        }
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Penalty configuration deactivated successfully."));
    }

    /// <summary>
    /// Xóa mềm (Soft Delete) cấu hình giá phạt.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteConfigAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Penalty configuration soft deleted successfully."));
    }
}

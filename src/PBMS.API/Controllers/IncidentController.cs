using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;

namespace PBMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpPost]
    public async Task<IActionResult> ReportIncident([FromBody] ReportIncidentRequest request)
    {
        var result = await _incidentService.ReportIncidentAsync(request);
        return Ok(BaseResponse<IncidentDto>.Ok(result, "Incident reported successfully."));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateIncidentStatus(int id, [FromBody] UpdateIncidentStatusRequest request)
    {
        var result = await _incidentService.UpdateIncidentStatusAsync(id, request);
        return Ok(BaseResponse<IncidentDto>.Ok(result, "Incident status updated successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIncidentById(int id)
    {
        var result = await _incidentService.GetIncidentByIdAsync(id);
        return Ok(BaseResponse<IncidentDto>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetIncidentsPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _incidentService.GetIncidentsPagedAsync(pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<IncidentDto>>.Ok(result));
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetIncidentsBySession(int sessionId)
    {
        var result = await _incidentService.GetIncidentsBySessionAsync(sessionId);
        return Ok(BaseResponse<IEnumerable<IncidentDto>>.Ok(result));
    }
}

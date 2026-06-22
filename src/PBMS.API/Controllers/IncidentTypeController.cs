using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;

namespace PBMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentTypeController : ControllerBase
{
    private readonly IIncidentTypeService _incidentTypeService;

    public IncidentTypeController(IIncidentTypeService incidentTypeService)
    {
        _incidentTypeService = incidentTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllIncidentTypes()
    {
        var result = await _incidentTypeService.GetAllIncidentTypesAsync();
        return Ok(BaseResponse<IEnumerable<IncidentTypeDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIncidentTypeById(int id)
    {
        var result = await _incidentTypeService.GetIncidentTypeByIdAsync(id);
        return Ok(BaseResponse<IncidentTypeDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateIncidentType([FromBody] CreateIncidentTypeRequest request)
    {
        var result = await _incidentTypeService.CreateIncidentTypeAsync(request);
        var response = BaseResponse<IncidentTypeDto>.Ok(result, "Incident type created successfully.");
        return CreatedAtAction(nameof(GetIncidentTypeById), new { id = result.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncidentType(int id, [FromBody] UpdateIncidentTypeRequest request)
    {
        var result = await _incidentTypeService.UpdateIncidentTypeAsync(id, request);
        return Ok(BaseResponse<IncidentTypeDto>.Ok(result, "Incident type updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncidentType(int id)
    {
        await _incidentTypeService.DeleteIncidentTypeAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Incident type deleted successfully."));
    }
}

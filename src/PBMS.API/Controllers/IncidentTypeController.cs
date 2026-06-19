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
}

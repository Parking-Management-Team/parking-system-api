using Microsoft.AspNetCore.Mvc;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;

namespace PBMS.API.Controllers;

[ApiController]
[Route("api/parking-sessions")]
public class ParkingSessionsController : ControllerBase
{
    private readonly IParkingSessionService _service;

    public ParkingSessionsController(IParkingSessionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParkingSessionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(request);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result)
            : ToErrorResult(result.ErrorCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _service.GetActiveAsync();
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpPatch("{id:int}/slot")]
    public async Task<IActionResult> AssignSlot(int id, [FromBody] AssignParkingSessionSlotRequest request)
    {
        var result = await _service.AssignSlotAsync(id, request);
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpPatch("{id:int}/checkout/start")]
    public async Task<IActionResult> StartCheckout(int id, [FromBody] StartCheckoutRequest request)
    {
        var result = await _service.StartCheckoutAsync(id, request);
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var result = await _service.CompleteAsync(id);
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpPatch("{id:int}/checkout/rollback")]
    public async Task<IActionResult> RollbackCheckout(int id)
    {
        var result = await _service.RollbackCheckoutAsync(id);
        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    private IActionResult ToErrorResult(string? errorCode, object result)
    {
        return errorCode switch
        {
            "NOT_FOUND" => NotFound(result),
            "VEHICLE_IN_ACTIVE_SESSION" or "CARD_IN_ACTIVE_SESSION" or "SLOT_IN_ACTIVE_SESSION" => Conflict(result),
            _ => BadRequest(result)
        };
    }
}

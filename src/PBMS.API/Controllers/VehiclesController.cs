using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// API controller for Driver Account & Vehicle Management.
/// </summary>
[ApiController]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? accountId)
    {
        _logger.LogInformation("Getting vehicles. AccountId: {AccountId}", accountId);

        var result = accountId.HasValue
            ? await _vehicleService.GetByAccountIdAsync(accountId.Value)
            : await _vehicleService.GetAllAsync();

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Getting vehicle with ID {VehicleId}", id);
        var result = await _vehicleService.GetByIdAsync(id);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating vehicle with license plate {LicensePlate}", createDto.LicensePlate);
        var result = await _vehicleService.CreateAsync(createDto);

        if (!result.Success)
        {
            return ToErrorResult(result.ErrorCode, result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating vehicle with ID {VehicleId}", id);
        var result = await _vehicleService.UpdateAsync(id, updateDto);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Archive(int id)
    {
        _logger.LogInformation("Archiving vehicle with ID {VehicleId}", id);
        var result = await _vehicleService.ArchiveAsync(id);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    private IActionResult ToErrorResult(string? errorCode, object result)
    {
        return errorCode switch
        {
            "NOT_FOUND" or "ACCOUNT_NOT_FOUND" or "VEHICLE_TYPE_NOT_FOUND" => NotFound(result),
            "LICENSE_PLATE_EXISTS" => Conflict(result),
            "VEHICLE_IN_ACTIVE_SESSION" => Conflict(result),
            "INTERNAL_ERROR" => StatusCode(StatusCodes.Status500InternalServerError, result),
            _ => BadRequest(result)
        };
    }
}

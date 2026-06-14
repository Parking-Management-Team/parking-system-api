using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// API controller for managing vehicle types.
/// Provides endpoints for CRUD operations on vehicle types.
/// </summary>
[ApiController]
[Route("api/vehicle-types")]
public class VehicleTypeController : ControllerBase
{
    private readonly IVehicleTypeService _vehicleTypeService;
    private readonly ILogger<VehicleTypeController> _logger;

    public VehicleTypeController(IVehicleTypeService vehicleTypeService, ILogger<VehicleTypeController> logger)
    {
        _vehicleTypeService = vehicleTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicle types.
    /// </summary>
    /// <returns>List of all vehicle types with their status.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all vehicle types");
        var result = await _vehicleTypeService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a specific vehicle type by ID.
    /// </summary>
    /// <param name="id">The vehicle type ID.</param>
    /// <returns>Vehicle type details.</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Getting vehicle type with ID {VehicleTypeId}", id);
        var result = await _vehicleTypeService.GetByIdAsync(id);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    /// <summary>
    /// Create a new vehicle type.
    /// </summary>
    /// <param name="createDto">Data for creating the vehicle type.</param>
    /// <returns>The created vehicle type.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleTypeDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new vehicle type with name: {Name}", createDto.Name);
        var result = await _vehicleTypeService.CreateAsync(createDto);

        if (!result.Success)
        {
            return ToErrorResult(result.ErrorCode, result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update an existing vehicle type.
    /// </summary>
    /// <param name="id">The vehicle type ID to update.</param>
    /// <param name="updateDto">Updated data for the vehicle type.</param>
    /// <returns>The updated vehicle type.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleTypeDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating vehicle type with ID {VehicleTypeId}", id);
        var result = await _vehicleTypeService.UpdateAsync(id, updateDto);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    /// <summary>
    /// Delete a vehicle type.
    /// </summary>
    /// <param name="id">The vehicle type ID to delete.</param>
    /// <returns>Success message or error if vehicle type is in use.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting vehicle type with ID {VehicleTypeId}", id);
        var result = await _vehicleTypeService.DeleteAsync(id);

        return result.Success ? Ok(result) : ToErrorResult(result.ErrorCode, result);
    }

    private IActionResult ToErrorResult(string? errorCode, object result)
    {
        return errorCode switch
        {
            "NOT_FOUND" => NotFound(result),
            "NAME_EXISTS" => Conflict(result),
            "IN_USE_SESSIONS" or "IN_USE_BOOKINGS" => Conflict(result),
            "INTERNAL_ERROR" => StatusCode(StatusCodes.Status500InternalServerError, result),
            _ => BadRequest(result)
        };
    }
}


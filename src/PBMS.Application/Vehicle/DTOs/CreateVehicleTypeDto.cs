namespace PBMS.Application.Vehicle.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO for creating a new vehicle type.
/// </summary>
public class CreateVehicleTypeDto
{
    /// <summary>
    /// Name of the vehicle type (e.g., "Bike", "Car").
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the vehicle type.
    /// </summary>
    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Business status of the vehicle type. Defaults to ACTIVE.
    /// </summary>
    [RegularExpression("(?i)^(ACTIVE|INACTIVE)$", ErrorMessage = "VehicleTypeStatus must be ACTIVE or INACTIVE.")]
    public string? VehicleTypeStatus { get; set; }

    [Range(0, 100, ErrorMessage = "BufferRatio must be between 0 and 100.")]
    public int BufferRatio { get; set; } = 10;
}

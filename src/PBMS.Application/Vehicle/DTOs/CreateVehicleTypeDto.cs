namespace PBMS.Application.Vehicle.DTOs;

/// <summary>
/// DTO for creating a new vehicle type.
/// </summary>
public class CreateVehicleTypeDto
{
    /// <summary>
    /// Name of the vehicle type (e.g., "Bike", "Car").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the vehicle type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Business status of the vehicle type. Defaults to ACTIVE.
    /// </summary>
    public string? VehicleTypeStatus { get; set; }
}

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
}

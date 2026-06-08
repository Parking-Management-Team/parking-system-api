namespace PBMS.Application.Vehicle.DTOs;

/// <summary>
/// DTO for updating an existing vehicle type.
/// </summary>
public class UpdateVehicleTypeDto
{
    /// <summary>
    /// Name of the vehicle type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the vehicle type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Business status of the vehicle type.
    /// </summary>
    public string? VehicleTypeStatus { get; set; }

}

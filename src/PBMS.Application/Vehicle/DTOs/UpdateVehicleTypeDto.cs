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
    /// Indicates if the vehicle type is active (True) or inactive (False).
    /// </summary>
    public bool IsActive { get; set; }
}

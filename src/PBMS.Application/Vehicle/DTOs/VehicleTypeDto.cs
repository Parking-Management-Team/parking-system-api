namespace PBMS.Application.Vehicle.DTOs;

/// <summary>
/// DTO for returning vehicle type data.
/// </summary>
public class VehicleTypeDto
{
    /// <summary>
    /// Unique identifier of the vehicle type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the vehicle type (e.g., "Bike", "Car").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status label: "Active" or "Inactive".
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Indicates if the vehicle type is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the vehicle type was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

namespace PBMS.Domain.Entities;

/// <summary>
/// Represents a parking session - when a vehicle parks in the facility.
/// </summary>
public class ParkingSession : BaseEntity
{
    /// <summary>
    /// Foreign key to the vehicle.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Navigation property to the vehicle.
    /// </summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>
    /// Indicates if the parking session is completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;
}
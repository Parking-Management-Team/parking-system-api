namespace PBMS.Domain.Entities;

/// <summary>
/// Represents a vehicle in the parking system.
/// </summary>
public class Vehicle : BaseEntity
{
    /// <summary>
    /// Foreign key to the vehicle type.
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Navigation property to the vehicle type.
    /// </summary>
    public VehicleType? VehicleType { get; set; }
}
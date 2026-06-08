namespace PBMS.Domain.Entities;

/// <summary>
/// Represents a type of vehicle.
/// Physical model columns: vehicle_type_id, type_name, description, vehicle_type_status.
/// </summary>
public class VehicleType : BaseEntity
{
    public const string StatusActive = "ACTIVE";
    public const string StatusInactive = "INACTIVE";
    public const string MotorcycleTypeName = "Motorcycle";
    public const string CarTypeName = "Car";

    /// <summary>
    /// Maps to type_name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maps to description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Maps to vehicle_type_status.
    /// </summary>
    public string VehicleTypeStatus { get; set; } = StatusActive;
}

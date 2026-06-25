namespace PBMS.Application.Vehicle.DTOs;

/// <summary>
/// DTO for returning vehicle_type data.
/// </summary>
public class VehicleTypeDto
{
    /// <summary>
    /// Maps to vehicle_type_id.
    /// </summary>
    public int Id { get; set; }

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
    public string VehicleTypeStatus { get; set; } = "ACTIVE";

    public int BufferRatio { get; set; } = 10;
}

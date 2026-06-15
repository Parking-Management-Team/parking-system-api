namespace PBMS.Application.Vehicle.DTOs;

/// <summary>
/// DTO for returning vehicle data.
/// </summary>
public class VehicleDto
{
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public int VehicleTypeId { get; set; }

    public string? VehicleTypeName { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public DateTime? RegisteredDay { get; set; }

    public string VehicleStatus { get; set; } = string.Empty;
}

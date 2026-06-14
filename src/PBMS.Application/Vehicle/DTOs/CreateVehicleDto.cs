namespace PBMS.Application.Vehicle.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO for adding a vehicle to an account or pre-registering a walk-in vehicle.
/// </summary>
public class CreateVehicleDto
{
    [Range(1, int.MaxValue, ErrorMessage = "AccountId must be greater than 0 when provided.")]
    public int? AccountId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "LicensePlate is required.")]
    [StringLength(20, ErrorMessage = "LicensePlate cannot exceed 20 characters.")]
    public string LicensePlate { get; set; } = string.Empty;

    public DateTime? RegisteredDay { get; set; }

    [RegularExpression("(?i)^(ACTIVE|INACTIVE|PENDING|SUSPENDED|ARCHIVED)$",
        ErrorMessage = "VehicleStatus must be ACTIVE, INACTIVE, PENDING, SUSPENDED, or ARCHIVED.")]
    public string? VehicleStatus { get; set; }
}

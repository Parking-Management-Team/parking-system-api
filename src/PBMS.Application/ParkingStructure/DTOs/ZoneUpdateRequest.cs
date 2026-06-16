using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để cập nhật zone hiện có.
/// </summary>
public class ZoneUpdateRequest
{
    [Required(ErrorMessage = "Code is required.")]
    [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters.")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "VehicleTypeId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Zone access type is required.")]
    [RegularExpression("GENERAL|MONTHLY", ErrorMessage = "Zone access type must be GENERAL or MONTHLY.")]
    public string ZoneAccessType { get; set; } = "GENERAL";
}

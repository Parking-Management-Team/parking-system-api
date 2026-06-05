using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để cập nhật zone hiện có.
/// </summary>
public class ZoneUpdateRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
    public int Capacity { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để tạo vị trí đỗ xe (Parking Slot) mới.
/// </summary>
public class ParkingSlotCreateRequest
{
    [Required(ErrorMessage = "ZoneId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be greater than 0.")]
    public int ZoneId { get; set; }

    [Required(ErrorMessage = "VehicleTypeId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Slot code is required.")]
    [MaxLength(20, ErrorMessage = "Slot code cannot exceed 20 characters.")]
    public string Code { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string? Name { get; set; }
}

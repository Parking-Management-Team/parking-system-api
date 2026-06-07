using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để tạo tầng (Floor) mới.
/// </summary>
public class FloorCreateRequest
{
    [Required(ErrorMessage = "BuildingId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "BuildingId must be greater than 0.")]
    public int BuildingId { get; set; }

    [Required(ErrorMessage = "Floor number is required.")]
    public int FloorNumber { get; set; }

    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string? Name { get; set; }
}

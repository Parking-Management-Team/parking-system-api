using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để tạo tòa nhà (Building) mới.
/// </summary>
public class BuildingCreateRequest
{
    [Required(ErrorMessage = "Building name is required.")]
    [MaxLength(50, ErrorMessage = "Building name cannot exceed 50 characters.")]
    public string Name { get; set; } = null!;

    [MaxLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Total floor is required.")]
    [Range(1, 100, ErrorMessage = "Total floor must be between 1 and 100.")]
    public int TotalFloor { get; set; }
}

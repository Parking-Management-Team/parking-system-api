using System.ComponentModel.DataAnnotations;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để cập nhật thông tin tòa nhà (Building).
/// </summary>
public class BuildingUpdateRequest
{
    [Required(ErrorMessage = "Building name is required.")]
    [MaxLength(50, ErrorMessage = "Building name cannot exceed 50 characters.")]
    public string Name { get; set; } = null!;

    [MaxLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Total floor is required.")]
    [Range(1, 100, ErrorMessage = "Total floor must be between 1 and 100.")]
    public int TotalFloor { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public BuildingStatus Status { get; set; }
}

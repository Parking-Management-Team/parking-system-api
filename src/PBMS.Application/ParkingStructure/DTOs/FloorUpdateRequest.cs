using System.ComponentModel.DataAnnotations;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để cập nhật thông tin tầng (Floor).
/// </summary>
public class FloorUpdateRequest
{
    [Required(ErrorMessage = "Floor number is required.")]
    public int FloorNumber { get; set; }

    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public FloorStatus Status { get; set; }
}

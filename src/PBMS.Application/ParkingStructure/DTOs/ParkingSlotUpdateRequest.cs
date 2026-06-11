using System.ComponentModel.DataAnnotations;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để cập nhật thông tin vị trí đỗ xe (Parking Slot).
/// </summary>
public class ParkingSlotUpdateRequest
{
    [Required(ErrorMessage = "Slot code is required.")]
    [MaxLength(20, ErrorMessage = "Slot code cannot exceed 20 characters.")]
    public string Code { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "VehicleTypeId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public SlotStatus Status { get; set; }
}

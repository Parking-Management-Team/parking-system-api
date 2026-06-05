using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để tạo zone mới.
/// </summary>
public class ZoneCreateRequest
{
    [Required(ErrorMessage = "FloorId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "FloorId phải lớn hơn 0.")]
    public int FloorId { get; set; }

    [Required(ErrorMessage = "Name là bắt buộc.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "VehicleTypeId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId phải lớn hơn 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Capacity là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity phải lớn hơn 0.")]
    public int Capacity { get; set; }
}
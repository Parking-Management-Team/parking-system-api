using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

public class CheckEntryRequest
{
    [Required(ErrorMessage = "License plate is required.")]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = null!;

    [Required(ErrorMessage = "VehicleTypeId is required.")]
    [Range(1, int.MaxValue)]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Card code is required.")]
    [MaxLength(20)]
    public string CardCode { get; set; } = null!;

    public int? BuildingId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

public class CheckInRequest
{
    [Required(ErrorMessage = "License plate is required.")]
    [MaxLength(20, ErrorMessage = "License plate cannot exceed 20 characters.")]
    public string LicensePlate { get; set; } = null!;

    [Required(ErrorMessage = "VehicleTypeId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleTypeId must be greater than 0.")]
    public int VehicleTypeId { get; set; }

    [Required(ErrorMessage = "Card code is required.")]
    [MaxLength(20, ErrorMessage = "Card code cannot exceed 20 characters.")]
    public string CardCode { get; set; } = null!;

    public int? BuildingId { get; set; }

    public int? StaffId { get; set; }

    public int? BookingId { get; set; }

    public int? MonthlySubscriptionId { get; set; }

    /// <summary>
    /// When true, randomly assigns an available slot instead of the first one found.
    /// Applies only to CAR walk-in/booking check-in without a reserved slot.
    /// </summary>
    public bool RandomizeSlot { get; set; }
}

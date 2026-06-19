using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

public class CreateParkingSessionRequest
{
    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int BuildingId { get; set; }

    [Range(1, int.MaxValue)]
    public int CardId { get; set; }

    public int? ZoneId { get; set; }
    public int? SlotId { get; set; }
    public int? BookingId { get; set; }
    public int? MonthlySubscriptionId { get; set; }
    public int? InStaffId { get; set; }

    public DateTime? CheckInTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string LicensePlateIn { get; set; } = string.Empty;
}

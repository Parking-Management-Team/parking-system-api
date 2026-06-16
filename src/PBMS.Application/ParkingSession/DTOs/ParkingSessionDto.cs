namespace PBMS.Application.ParkingSession.DTOs;

public class ParkingSessionDto
{
    public int SessionId { get; set; }
    public int VehicleId { get; set; }
    public int BuildingId { get; set; }
    public int CardId { get; set; }
    public int? ZoneId { get; set; }
    public int? SlotId { get; set; }
    public int? BookingId { get; set; }
    public int? MonthlySubscriptionId { get; set; }
    public int? InStaffId { get; set; }
    public int? OutStaffId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string LicensePlateIn { get; set; } = string.Empty;
    public string? LicensePlateOut { get; set; }
    public string SessionStatus { get; set; } = "ACTIVE";
}

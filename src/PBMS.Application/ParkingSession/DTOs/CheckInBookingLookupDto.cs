namespace PBMS.Application.ParkingSession.DTOs;

public class CheckInBookingLookupDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int VehicleTypeId { get; set; }
    public string? VehicleTypeName { get; set; }
    public int BuildingId { get; set; }
    public string? BuildingName { get; set; }
    public DateTime PlannedCheckinTime { get; set; }
    public DateTime CheckinGraceUntil { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
}

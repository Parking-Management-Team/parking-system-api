namespace PBMS.Application.ParkingSession.DTOs;

public class ParkingSessionDto
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public int VehicleTypeId { get; set; }

    public int CardId { get; set; }

    public string CardCode { get; set; } = null!;

    public int? ZoneId { get; set; }

    public string? ZoneCode { get; set; }

    public int? ParkingSlotId { get; set; }

    public string? SlotCode { get; set; }

    public DateTime CheckInTime { get; set; }

    public string SessionStatus { get; set; } = null!;
}

using System.Text.Json.Serialization;

namespace PBMS.Application.ParkingSession.DTOs;

public sealed class ActiveParkingSessionSummaryDto : ParkingSessionDto
{
    [JsonIgnore]
    public int PricingVehicleTypeId { get; set; }

    [JsonIgnore]
    public DateTime? BookingPlannedCheckoutTime { get; set; }
}

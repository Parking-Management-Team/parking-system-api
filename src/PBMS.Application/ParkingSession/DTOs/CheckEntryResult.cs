namespace PBMS.Application.ParkingSession.DTOs;

public class CheckEntryResult
{
    public bool Allowed { get; set; }
    public string? Reason { get; set; }
    public bool PricingPolicyValid { get; set; }
    public bool ZoneAvailable { get; set; }
    public bool CardAvailable { get; set; }
    public bool NotBlacklisted { get; set; }
    public bool NotAlreadyParked { get; set; }
}

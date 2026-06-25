namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO for subscription info displayed in Parking Slot context.
/// </summary>
public class SlotSubscriptionInfoDto
{
    public int SubscriptionId { get; set; }
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public int VehicleId { get; set; }
    public string? LicensePlate { get; set; }
    public string Status { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
}
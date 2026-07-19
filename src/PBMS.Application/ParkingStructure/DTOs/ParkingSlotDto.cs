using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO đại diện cho vị trí đỗ xe (Parking Slot) trong response.
/// </summary>
public class ParkingSlotDto
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public int VehicleTypeId { get; set; }
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public SlotStatus Status { get; set; }
    public string? OccupiedLicensePlate { get; set; }
    public SlotSubscriptionInfoDto? Subscription { get; set; }
    public bool IsReserved { get; set; }
}

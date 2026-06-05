namespace PBMS.Domain.Entities;

public class ParkingSlot : BaseEntity
{
    public int ZoneId { get; set; }
    public string SlotNumber { get; set; } = null!;
    public bool IsOccupied { get; set; }
    public Zone Zone { get; set; } = null!;
}
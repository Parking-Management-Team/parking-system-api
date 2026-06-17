namespace PBMS.Domain.Entities;

public class ParkingSession : BaseEntity
{
    public int VehicleId { get; set; }

    public int? ZoneId { get; set; }

    public int? ParkingSlotId { get; set; }

    public int? CardId { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    public DateTime? CheckOutTime { get; set; }

    public int? InStaffId { get; set; }

    public int? OutStaffId { get; set; }

    public string SessionStatus { get; set; } = "Active";

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual Zone? Zone { get; set; }

    public virtual ParkingSlot? ParkingSlot { get; set; }

    public virtual Card? Card { get; set; }
}

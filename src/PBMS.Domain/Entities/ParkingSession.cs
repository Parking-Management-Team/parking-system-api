namespace PBMS.Domain.Entities;

public class ParkingSession : BaseEntity
{
    public int VehicleId { get; set; }

    public int BuildingId { get; set; }

    public int CardId { get; set; }

    public int? ZoneId { get; set; }

    public int? SlotId { get; set; }

    public int? BookingId { get; set; }

    public int? MonthlySubscriptionId { get; set; }

    public int? InStaffId { get; set; }

    public int? OutStaffId { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    public DateTime? CheckOutTime { get; set; }

    public string LicensePlateIn { get; set; } = null!;

    public string? LicensePlateOut { get; set; }

    public string SessionStatus { get; set; } = "ACTIVE";

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual Building Building { get; set; } = null!;

    public virtual Card Card { get; set; } = null!;

    public virtual Zone? Zone { get; set; }

    public virtual ParkingSlot? ParkingSlot { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual MonthlySubscription? MonthlySubscription { get; set; }

    public virtual Account? InStaff { get; set; }

    public virtual Account? OutStaff { get; set; }

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

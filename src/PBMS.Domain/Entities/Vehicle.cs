namespace PBMS.Domain.Entities;

public class Vehicle : BaseEntity
{
    public const string StatusActive = "ACTIVE";
    public const string StatusInactive = "INACTIVE";
    public const string StatusPending = "PENDING";
    public const string StatusSuspended = "SUSPENDED";
    public const string StatusArchived = "ARCHIVED";

    public int? AccountId { get; set; }

    public int VehicleTypeId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public DateTime? RegisteredDay { get; set; }

    public string VehicleStatus { get; set; } = StatusActive;

    public virtual Account? Account { get; set; }

    public virtual VehicleType VehicleType { get; set; } = null!;

    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();

    public virtual ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();
}

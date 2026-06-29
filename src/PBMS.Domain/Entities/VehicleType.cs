namespace PBMS.Domain.Entities;

public class VehicleType : BaseEntity
{
    public const string StatusActive = "ACTIVE";
    public const string StatusInactive = "INACTIVE";
    public const string MotorcycleTypeName = "Motorcycle";
    public const string CarTypeName = "Car";

    public string TypeName { get; set; } = null!;

    public string VehicleTypeCode { get; set; } = null!;

    public string? Description { get; set; }

    public string VehicleTypeStatus { get; set; } = StatusActive;

    public int BufferRatio { get; set; } = 10;

    public virtual ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public virtual ICollection<PricingPolicy> PricingPolicies { get; set; } = new List<PricingPolicy>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

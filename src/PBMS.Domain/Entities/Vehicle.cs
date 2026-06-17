namespace PBMS.Domain.Entities;

public class Vehicle : BaseEntity
{
    public int VehicleTypeId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;

    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
}

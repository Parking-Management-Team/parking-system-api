using PBMS.Domain.Enums;
namespace PBMS.Domain.Entities;

public class Zone : BaseEntity
{
    public int FloorId { get; set; }
    public string Name { get; set; } = null!;
    public int VehicleTypeId { get; set; }
    public int Capacity { get; set; }
    public ZoneStatus Status { get; set; }
    public Floor Floor { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();
}
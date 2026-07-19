namespace PBMS.Application.ParkingStructure.DTOs;

public class CapacityDto
{
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int AvailableSlots => Math.Max(0, TotalSlots - OccupiedSlots);
}

public class FloorCapacityDetailDto
{
    public int FloorId { get; set; }
    public int FloorNumber { get; set; }
    public string? FloorName { get; set; }
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int AvailableSlots => Math.Max(0, TotalSlots - OccupiedSlots);
    public List<VehicleTypeCapacityDto> VehicleTypeCapacities { get; set; } = new();
}

public class VehicleTypeCapacityDto
{
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int AvailableSlots => Math.Max(0, TotalSlots - OccupiedSlots);
}

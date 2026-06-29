namespace PBMS.Application.ParkingStructure.DTOs;

public class CapacityDto
{
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int AvailableSlots => Math.Max(0, TotalSlots - OccupiedSlots);
}

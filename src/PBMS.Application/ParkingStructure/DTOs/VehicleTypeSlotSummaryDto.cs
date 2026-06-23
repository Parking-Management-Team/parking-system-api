namespace PBMS.Application.ParkingStructure.DTOs;

public class VehicleTypeSlotSummaryDto
{
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public int TotalSlots { get; set; }
    public List<SlotStatusCountDto> StatusCounts { get; set; } = new();
}

public class SlotStatusCountDto
{
    public string Status { get; set; } = null!;
    public int Count { get; set; }
}

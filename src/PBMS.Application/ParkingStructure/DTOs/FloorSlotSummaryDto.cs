using System.Collections.Generic;

namespace PBMS.Application.ParkingStructure.DTOs;

public class FloorSlotSummaryDto
{
    public int FloorId { get; set; }
    public int FloorNumber { get; set; }
    public int TotalSlots { get; set; }
    public List<VehicleTypeSlotSummaryDto> VehicleTypeSummaries { get; set; } = new();
}

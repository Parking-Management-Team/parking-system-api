using System.Collections.Generic;

namespace PBMS.Application.ParkingStructure.DTOs;

public class VehicleTypeSlotSummaryDto
{
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public int TotalSlots { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

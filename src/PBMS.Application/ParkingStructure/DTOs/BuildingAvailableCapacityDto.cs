using System.Collections.Generic;

namespace PBMS.Application.ParkingStructure.DTOs
{
    public class BuildingAvailableCapacityDto
    {
        public int BuildingId { get; set; }
        public List<VehicleTypeCapacityDetailDto> VehicleTypeCapacities { get; set; } = new();
    }

    public class VehicleTypeCapacityDetailDto
    {
        public int VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; } = null!;
        public int TotalCapacity { get; set; }
        public int BufferSlots { get; set; }
        public int EffectiveCapacity { get; set; }
        public int ActiveSessions { get; set; }
        public int ReservedBookings { get; set; }
        public int AvailableCapacity { get; set; }
    }
}

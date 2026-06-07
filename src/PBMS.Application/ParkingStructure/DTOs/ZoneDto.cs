using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để đại diện cho zone trong response.
/// </summary>
public class ZoneDto
{
    public int Id { get; set; }
    public int FloorId { get; set; }
    public string Name { get; set; } = null!;
    public int VehicleTypeId { get; set; }
    public int Capacity { get; set; }
    public ZoneStatus Status { get; set; }
}
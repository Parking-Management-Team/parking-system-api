using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO dùng để đại diện cho tầng (Floor) trong response.
/// </summary>
public class FloorDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public string? Name { get; set; }
    public FloorStatus Status { get; set; }
}

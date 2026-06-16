using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO đại diện cho tòa nhà (Building) trong response.
/// </summary>
public class BuildingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public int TotalFloor { get; set; }
    public BuildingStatus Status { get; set; }
}

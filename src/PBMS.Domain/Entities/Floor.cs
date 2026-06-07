using PBMS.Domain.Enums;
namespace PBMS.Domain.Entities;

public class Floor : BaseEntity
{
    public int BuildingId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = null!;
    public FloorStatus Status { get; set; }
    public Building Building { get; set; } = null!;
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}
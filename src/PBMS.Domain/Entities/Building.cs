using PBMS.Domain.Enums;
namespace PBMS.Domain.Entities;

public class Building : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int FloorCount { get; set; }
    public BuildingStatus Status { get; set; }
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
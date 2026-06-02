namespace PBMS.Domain.Entities;

public class Role : BaseEntity
{
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
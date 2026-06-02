namespace PBMS.Domain.Entities;

public class Account : BaseEntity
{
    public int RoleId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string AccountStatus { get; set; } = "Active";

    // Helper property to check active state based on status
    public bool IsActive => AccountStatus == "Active";

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
}
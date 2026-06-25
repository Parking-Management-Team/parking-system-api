namespace PBMS.Application.AuditLog.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string Action { get; set; } = null!;
    public string? TargetTable { get; set; }
    public int? TargetId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
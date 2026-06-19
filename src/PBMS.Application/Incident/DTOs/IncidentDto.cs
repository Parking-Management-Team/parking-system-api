using System;
using PBMS.Domain.Enums;

namespace PBMS.Application.Incident.DTOs;

public class IncidentDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string? LicensePlate { get; set; } // Lấy từ Session
    public int IncidentTypeId { get; set; }
    public string IncidentName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? PenaltyFee { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

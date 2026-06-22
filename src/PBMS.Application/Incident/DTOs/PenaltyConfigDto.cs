using System;

namespace PBMS.Application.Incident.DTOs;

public class PenaltyConfigDto
{
    public int Id { get; set; }
    public int IncidentTypeId { get; set; }
    public string IncidentTypeName { get; set; } = null!;
    public decimal PenaltyFee { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

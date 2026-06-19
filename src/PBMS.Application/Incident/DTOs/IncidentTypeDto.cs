namespace PBMS.Application.Incident.DTOs;

public class IncidentTypeDto
{
    public int Id { get; set; }
    public string IncidentCode { get; set; } = null!;
    public string IncidentName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DefaultPenaltyFee { get; set; }
}

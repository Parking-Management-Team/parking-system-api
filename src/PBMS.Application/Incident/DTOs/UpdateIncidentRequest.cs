namespace PBMS.Application.Incident.DTOs;

public class UpdateIncidentRequest
{
    public string? Description { get; set; }
    public decimal? PenaltyFee { get; set; }
    public int? IncidentTypeId { get; set; }
}

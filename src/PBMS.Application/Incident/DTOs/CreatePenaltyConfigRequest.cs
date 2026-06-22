namespace PBMS.Application.Incident.DTOs;

public class CreatePenaltyConfigRequest
{
    public int IncidentTypeId { get; set; }
    public decimal PenaltyFee { get; set; }
}

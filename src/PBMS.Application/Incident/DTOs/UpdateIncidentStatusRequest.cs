using PBMS.Domain.Enums;

namespace PBMS.Application.Incident.DTOs;

public class UpdateIncidentStatusRequest
{
    public IncidentStatus Status { get; set; }
    public string? Description { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Incident.DTOs;

public class ReportIncidentRequest
{
    [Required]
    public int SessionId { get; set; }

    [Required]
    public int IncidentTypeId { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    public decimal? PenaltyFee { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Incident.DTOs;

public class UpdateIncidentTypeRequest
{
    [Required]
    [MaxLength(100)]
    public string IncidentName { get; set; } = null!;

    public string? Description { get; set; }
}

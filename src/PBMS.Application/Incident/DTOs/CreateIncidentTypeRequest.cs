using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Incident.DTOs;

public class CreateIncidentTypeRequest
{
    [Required]
    [MaxLength(20)]
    public string IncidentCode { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string IncidentName { get; set; } = null!;

    public string? Description { get; set; }
}

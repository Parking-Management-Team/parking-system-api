using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.MonthlyCard.DTOs;

public class AssignCardRequest
{
    [Required]
    [MaxLength(20)]
    public string CardCode { get; set; } = string.Empty;
}

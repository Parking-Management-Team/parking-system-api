using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.MonthlyCard.DTOs;

public class RenewSubscriptionRequest
{
    [Required]
    [Range(1, 12, ErrorMessage = "Duration must be between 1 and 12 months.")]
    public int Months { get; set; }
}

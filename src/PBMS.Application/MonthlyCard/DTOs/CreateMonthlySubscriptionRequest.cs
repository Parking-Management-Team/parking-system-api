using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.MonthlyCard.DTOs;

public class CreateMonthlySubscriptionRequest
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int BuildingId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal MonthlyPrice { get; set; }
}

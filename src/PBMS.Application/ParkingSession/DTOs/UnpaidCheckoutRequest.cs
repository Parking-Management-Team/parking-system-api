using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

/// <summary>
/// Request to complete check-out without payment and add the vehicle to the blacklist.
/// </summary>
public class UnpaidCheckoutRequest
{
    [Required(ErrorMessage = "Staff ID is required.")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters.")]
    public string Reason { get; set; } = "The customer left without paying the parking fee.";
}

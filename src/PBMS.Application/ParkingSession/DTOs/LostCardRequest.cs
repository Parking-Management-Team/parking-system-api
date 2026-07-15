using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

/// <summary>
/// Request to report a lost card for a parking session.
/// </summary>
public class LostCardRequest
{
    [Required(ErrorMessage = "Staff ID is required.")]
    public int StaffId { get; set; }

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; } = "The customer reported a lost card at the exit gate.";
}

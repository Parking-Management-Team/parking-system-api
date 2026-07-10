using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

/// <summary>
/// Yêu cầu báo mất thẻ cho một phiên gửi xe.
/// </summary>
public class LostCardRequest
{
    [Required(ErrorMessage = "Staff ID is required.")]
    public int StaffId { get; set; }

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string Description { get; set; } = "Khách báo mất thẻ tại cổng ra.";
}

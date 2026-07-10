using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

/// <summary>
/// Yêu cầu check-out không thanh toán (chưa đóng tiền, bị đưa vào Blacklist).
/// </summary>
public class UnpaidCheckoutRequest
{
    [Required(ErrorMessage = "Staff ID is required.")]
    public int StaffId { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters.")]
    public string Reason { get; set; } = "Khách hàng không thanh toán phí gửi xe và bỏ đi.";
}

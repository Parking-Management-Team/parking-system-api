using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Booking.DTOs;

/// <summary>
/// DTO yêu cầu cập nhật thời gian đặt chỗ (Request Object).
///
/// Dùng cho endpoint: PUT /api/bookings/{id}
/// Chỉ cho phép cập nhật PlannedCheckinTime khi Booking đang PENDING.
/// </summary>
public class UpdateBookingRequest
{
    /// <summary>
    /// Thời gian dự kiến vào bãi mới (Giờ Việt Nam).
    /// Phải cách hiện tại tối thiểu 15 phút.
    /// </summary>
    [Required]
    public DateTime PlannedCheckinTime { get; set; }

    /// <summary>
    /// Thời gian dự kiến ra bãi mới (Giờ Việt Nam).
    /// Phải cách thời gian vào bãi tối thiểu 4 tiếng.
    /// </summary>
    [Required]
    public DateTime PlannedCheckoutTime { get; set; }
}

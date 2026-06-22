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
    /// Thời gian dự kiến vào bãi mới (UTC).
    /// Phải từ 1 đến 8 tiếng tính từ thời điểm gửi request.
    /// </summary>
    [Required]
    public DateTime PlannedCheckinTime { get; set; }
}

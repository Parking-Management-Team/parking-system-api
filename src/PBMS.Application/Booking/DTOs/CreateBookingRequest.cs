using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Booking.DTOs;

/// <summary>
/// DTO yêu cầu tạo Booking mới từ phía Driver (Request Object).
///
/// Dùng cho endpoint: POST /api/bookings
/// </summary>
public class CreateBookingRequest
{
    /// <summary>
    /// ID tài khoản Driver thực hiện đặt chỗ.
    /// </summary>
    [Required]
    public int AccountId { get; set; }

    /// <summary>
    /// Biển số xe cần đặt chỗ. Hệ thống sẽ tự tra cứu VehicleId và VehicleType.
    /// Ví dụ: "51G-12345"
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = null!;

    /// <summary>
    /// ID Tòa nhà muốn đặt chỗ.
    /// </summary>
    [Required]
    public int BuildingId { get; set; }

    /// <summary>
    /// Thời gian dự kiến vào bãi (UTC/Local tùy chỉnh).
    /// Phải từ 1 đến 8 tiếng tính từ thời điểm gửi request.
    /// </summary>
    [Required]
    public DateTime PlannedCheckinTime { get; set; }

    /// <summary>
    /// Thời gian dự kiến ra khỏi bãi.
    /// </summary>
    [Required]
    public DateTime PlannedCheckoutTime { get; set; }
}

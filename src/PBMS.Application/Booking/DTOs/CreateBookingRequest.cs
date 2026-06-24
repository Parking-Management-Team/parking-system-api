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
    /// Thời gian dự kiến vào bãi (Giờ Việt Nam).
    /// Phải cách hiện tại tối thiểu 15 phút.
    /// </summary>
    [Required]
    public DateTime PlannedCheckinTime { get; set; }

    /// <summary>
    /// Thời gian dự kiến ra bãi (Giờ Việt Nam).
    /// Phải cách thời gian vào bãi tối thiểu 4 tiếng.
    /// </summary>
    [Required]
    public DateTime PlannedCheckoutTime { get; set; }
}

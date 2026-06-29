namespace PBMS.Application.Booking.DTOs;

/// <summary>
/// DTO trả về thông tin Booking cho Client (Response Object).
///
/// Dùng cho tất cả các endpoint trả về thông tin Booking:
///   - GET  /api/bookings
///   - GET  /api/bookings/{id}
///   - POST /api/bookings (sau khi tạo)
///   - PUT  /api/bookings/{id} (sau khi cập nhật)
/// </summary>
public class BookingDto
{
    /// <summary>ID định danh duy nhất của Booking.</summary>
    public int Id { get; set; }

    /// <summary>ID tài khoản Driver đặt chỗ.</summary>
    public int AccountId { get; set; }

    /// <summary>Tên tài khoản Driver (nếu có).</summary>
    public string? AccountName { get; set; }

    /// <summary>ID xe được đặt chỗ.</summary>
    public int VehicleId { get; set; }

    /// <summary>Biển số xe đặt chỗ.</summary>
    public string LicensePlate { get; set; } = null!;

    /// <summary>ID loại xe tại thời điểm đặt.</summary>
    public int VehicleTypeId { get; set; }

    /// <summary>Tên loại xe (Ví dụ: "Car", "Motorcycle").</summary>
    public string? VehicleTypeName { get; set; }

    /// <summary>ID Tòa nhà được đặt chỗ.</summary>
    public int BuildingId { get; set; }

    /// <summary>Tên Tòa nhà.</summary>
    public string? BuildingName { get; set; }

    /// <summary>Thời gian dự kiến vào bãi.</summary>
    public DateTime PlannedCheckinTime { get; set; }

    /// <summary>
    /// Số tiền đặt cọc (bằng BasePrice của PricingWindow tại giờ check-in dự kiến).
    /// </summary>
    public decimal DepositAmount { get; set; }

    /// <summary>
    /// Trạng thái Booking hiện tại.
    /// Các giá trị: "Pending", "Confirmed", "CheckedIn", "Cancelled", "NoShow", "Expired"
    /// </summary>
    public string BookingStatus { get; set; } = null!;

    /// <summary>Hạn cuối để thanh toán tiền cọc.</summary>
    public DateTime PaymentDeadline { get; set; }

    /// <summary>Hạn grace period sau PlannedCheckinTime để xe thực sự vào bãi.</summary>
    public DateTime CheckinGraceUntil { get; set; }

    /// <summary>Thời điểm xác nhận Booking (sau khi thanh toán thành công). Null nếu chưa xác nhận.</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Thời điểm hủy Booking. Null nếu chưa hủy.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Lý do hủy Booking. Null nếu chưa hủy.</summary>
    public string? CancelReason { get; set; }

    /// <summary>Thời điểm tạo Booking.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>ID vị trí đỗ xe chọn trước (chỉ áp dụng cho xe hơi).</summary>
    public int? SlotId { get; set; }

    /// <summary>Mã vị trí đỗ xe chọn trước (Ví dụ: "SLOT-A01").</summary>
    public string? SlotCode { get; set; }
}

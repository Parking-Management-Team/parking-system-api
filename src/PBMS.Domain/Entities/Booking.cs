namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể đặt chỗ trước (Booking) của khách hàng.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.16 — Physical Model: booking
/// </summary>
public class Booking : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tài khoản thực hiện đặt chỗ (Account).
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới phương tiện được đăng ký đặt chỗ (Vehicle).
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện tại thời điểm đặt (VehicleType).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới tòa nhà áp dụng đặt chỗ (Building).
    /// </summary>
    public int BuildingId { get; set; }

    /// <summary>
    /// Thời gian dự kiến vào bãi (Check-in).
    /// </summary>
    public DateTime PlannedCheckinTime { get; set; }

    /// <summary>
    /// Thời gian dự kiến ra khỏi bãi (Check-out).
    /// </summary>
    public DateTime PlannedCheckoutTime { get; set; }

    /// <summary>
    /// Số tiền đặt cọc (Deposit fee).
    /// Bằng giá của block đầu tiên theo bảng giá hiện hành tại thời điểm đặt.
    /// </summary>
    public decimal DepositAmount { get; set; }

    /// <summary>
    /// Trạng thái của quy trình đặt chỗ (Ví dụ: "Pending", "Confirmed", "Cancelled", "Completed", "Expired").
    /// </summary>
    public string BookingStatus { get; set; } = "Pending";

    /// <summary>
    /// Hạn cuối cùng để thanh toán khoản đặt cọc.
    /// </summary>
    public DateTime PaymentDeadline { get; set; }

    /// <summary>
    /// Hạn cuối cùng cho phép xe check-in (Sau thời gian này booking sẽ hết hạn/No-show).
    /// </summary>
    public DateTime CheckinGraceUntil { get; set; }

    /// <summary>
    /// Thời điểm hủy đặt chỗ (nếu có).
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Lý do hủy đặt chỗ.
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// Thời điểm xác nhận đặt chỗ thành công (sau khi đã thanh toán tiền cọc).
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tài khoản đặt chỗ.
    /// </summary>
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    /// Thông tin phương tiện được đặt chỗ.
    /// </summary>
    public virtual Vehicle Vehicle { get; set; } = null!;

    /// <summary>
    /// Thông tin loại phương tiện tại thời điểm đặt.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Thông tin tòa nhà đặt chỗ.
    /// </summary>
    public virtual Building Building { get; set; } = null!;

    /// <summary>
    /// Danh sách các giao dịch thanh toán liên quan đến đặt chỗ này.
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

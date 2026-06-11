namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Giao dịch thanh toán (Payment) trong hệ thống.
/// Lưu các khoản thu tiền từ Lượt gửi xe, Đặt chỗ trước, hoặc Vé tháng.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.20 — Physical Model: payment
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới lượt gửi xe (ParkingSession) nếu đây là giao dịch thanh toán gửi xe vãng lai hoặc trả sau.
    /// </summary>
    public int? SessionId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới đặt chỗ trước (Booking) nếu đây là giao dịch thanh toán đặt cọc.
    /// </summary>
    public int? BookingId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới hồ sơ đăng ký vé tháng (MonthlySubscription) nếu đây là giao dịch thanh toán mua/gia hạn vé tháng.
    /// </summary>
    public int? MonthlySubscriptionId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới chính sách giá (PricingPolicy) áp dụng để tính tiền tại thời điểm thanh toán.
    /// Dùng để lưu vết phục vụ đối soát khi bảng giá thay đổi.
    /// </summary>
    public int? PricingPolicyId { get; set; }

    /// <summary>
    /// Số tiền thanh toán thực tế của giao dịch.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Phương thức thanh toán (Ví dụ: "CASH", "ONLINE_BANKING").
    /// </summary>
    public string PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Thời điểm giao dịch được thực hiện thành công (Null nếu đang chờ thanh toán).
    /// </summary>
    public DateTime? PaymentTime { get; set; }

    /// <summary>
    /// Trạng thái của giao dịch (Ví dụ: "PENDING", "PAID", "FAILED", "REFUNDED").
    /// </summary>
    public string PaymentStatus { get; set; } = "PENDING";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin lượt gửi xe liên quan.
    /// </summary>
    public virtual ParkingSession? Session { get; set; }

    /// <summary>
    /// Thông tin đặt chỗ liên quan.
    /// </summary>
    public virtual Booking? Booking { get; set; }

    /// <summary>
    /// Thông tin vé tháng liên quan.
    /// </summary>
    public virtual MonthlySubscription? MonthlySubscription { get; set; }

    /// <summary>
    /// Thông tin chính sách giá được áp dụng.
    /// </summary>
    public virtual PricingPolicy? PricingPolicy { get; set; }

    /// <summary>
    /// Danh sách liên kết Nhiều-Nhiều thông qua bảng trung gian để đối soát thống kê doanh thu.
    /// </summary>
    public virtual ICollection<RevenueStatisticPayment> RevenueStatisticPayments { get; set; } = new List<RevenueStatisticPayment>();
}

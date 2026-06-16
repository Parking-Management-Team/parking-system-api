namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể thống kê doanh thu (Revenue Statistic) theo chu kỳ.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.21 — Physical Model: revenue_statistic
/// </summary>
public class RevenueStatistic : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tòa nhà/bãi xe được thống kê (Building).
    /// </summary>
    public DateOnly StatDate { get; set; }

    /// <summary>
    /// Ngày bắt đầu chu kỳ thống kê.
    /// </summary>
    public int? VehicleTypeId { get; set; }

    /// <summary>
    /// Ngày kết thúc chu kỳ thống kê.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Loại chu kỳ thống kê (Ví dụ: "DAILY", "MONTHLY", "YEARLY").
    /// </summary>
    public int TotalPaymentsCount { get; set; }

    /// <summary>
    /// Tổng doanh thu thực nhận trong chu kỳ này.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Tổng số lượt đặt chỗ (Booking) đã hoàn thành trong chu kỳ này.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tổng số lượt gửi xe (ParkingSession) đã hoàn thành trong chu kỳ này.
    /// </summary>

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tòa nhà/bãi xe được thống kê.
    /// </summary>
    public virtual VehicleType? VehicleType { get; set; }

    /// <summary>
    /// Danh sách liên kết Nhiều-Nhiều thông qua bảng trung gian với các giao dịch thanh toán (Payment).
    /// </summary>
    public virtual ICollection<RevenueStatisticPayment> RevenueStatisticPayments { get; set; } = new List<RevenueStatisticPayment>();
}

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
    public int BuildingId { get; set; }

    /// <summary>
    /// Ngày bắt đầu chu kỳ thống kê.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Ngày kết thúc chu kỳ thống kê.
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Loại chu kỳ thống kê (Ví dụ: "DAILY", "MONTHLY", "YEARLY").
    /// </summary>
    public string PeriodType { get; set; } = null!;

    /// <summary>
    /// Tổng doanh thu thực nhận trong chu kỳ này.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Tổng số lượt đặt chỗ (Booking) đã hoàn thành trong chu kỳ này.
    /// </summary>
    public int TotalBookings { get; set; }

    /// <summary>
    /// Tổng số lượt gửi xe (ParkingSession) đã hoàn thành trong chu kỳ này.
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// Tổng số lượt đăng ký/gia hạn vé tháng (MonthlySubscription) thành công trong chu kỳ này.
    /// </summary>
    public int TotalSubscriptions { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện (VehicleType) nếu thống kê theo loại xe.
    /// Để null nếu là dòng tổng doanh thu chung (overall total).
    /// </summary>
    public int? VehicleTypeId { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tòa nhà/bãi xe được thống kê.
    /// </summary>
    public virtual Building Building { get; set; } = null!;

    /// <summary>
    /// Danh sách liên kết Nhiều-Nhiều thông qua bảng trung gian với các giao dịch thanh toán (Payment).
    /// </summary>
    public virtual ICollection<RevenueStatisticPayment> RevenueStatisticPayments { get; set; } = new List<RevenueStatisticPayment>();

    /// <summary>
    /// Thông tin loại phương tiện được thống kê.
    /// </summary>
    public virtual VehicleType? VehicleType { get; set; }

}

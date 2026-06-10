namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể liên kết trung gian Nhiều-Nhiều (Many-to-Many Join Entity) giữa RevenueStatistic và Payment.
/// </summary>
public class RevenueStatisticPayment
{
    /// <summary>
    /// Khóa ngoại liên kết tới dòng thống kê doanh thu (RevenueStatistic). Part of Composite PK.
    /// </summary>
    public int StatisticId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới giao dịch thanh toán (Payment). Part of Composite PK.
    /// </summary>
    public int PaymentId { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin thống kê doanh thu.
    /// </summary>
    public virtual RevenueStatistic RevenueStatistic { get; set; } = null!;

    /// <summary>
    /// Thông tin giao dịch thanh toán.
    /// </summary>
    public virtual Payment Payment { get; set; } = null!;
}

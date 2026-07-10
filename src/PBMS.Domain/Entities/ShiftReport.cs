using System;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Báo cáo ca trực (ShiftReport) của nhân viên cổng (Staff).
/// Kế thừa từ BaseEntity và ISoftDeletable.
/// </summary>
public class ShiftReport : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// ID tài khoản nhân viên (Staff) lập báo cáo.
    /// </summary>
    public int StaffId { get; set; }

    /// <summary>
    /// Thời điểm bắt đầu ca trực.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Thời điểm kết thúc ca trực (nộp báo cáo).
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Tổng số lượt xe cho vào bãi (Check-in) trong ca.
    /// </summary>
    public int TotalCheckIn { get; set; }

    /// <summary>
    /// Tổng số lượt xe cho ra bãi (Check-out) trong ca.
    /// </summary>
    public int TotalCheckOut { get; set; }

    /// <summary>
    /// Tổng doanh thu ghi nhận từ hệ thống trong ca trực (gồm CASH và ONLINE_BANKING).
    /// </summary>
    public decimal SystemRevenue { get; set; }

    /// <summary>
    /// Số tiền mặt dự kiến phải thu được (thanh toán bằng CASH).
    /// </summary>
    public decimal ExpectedCashAmount { get; set; }

    /// <summary>
    /// Số tiền mặt thực tế nhân viên kiểm đếm được trong két.
    /// </summary>
    public decimal ActualCashAmount { get; set; }

    /// <summary>
    /// Chênh lệch tiền mặt (ActualCashAmount - ExpectedCashAmount).
    /// </summary>
    public decimal DifferenceAmount { get; set; }

    /// <summary>
    /// Ghi chú giải trình của nhân viên (đặc biệt khi có chênh lệch tiền).
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Trạng thái phê duyệt báo cáo ca (Ví dụ: "Submitted", "Approved", "Rejected").
    /// Mặc định là "Submitted" khi gửi lên.
    /// </summary>
    public string Status { get; set; } = "Submitted";

    /// <summary>
    /// ID tài khoản Quản lý (Manager) thực hiện duyệt báo cáo ca.
    /// </summary>
    public int? ApprovedById { get; set; }

    /// <summary>
    /// Thời điểm báo cáo ca được duyệt.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    // -----------------------------------------------------------------------
    // SOFT DELETE PROPERTIES
    // -----------------------------------------------------------------------
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------
    public virtual Account Staff { get; set; } = null!;
    public virtual Account? ApprovedBy { get; set; }
}

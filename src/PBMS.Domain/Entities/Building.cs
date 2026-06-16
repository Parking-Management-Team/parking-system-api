using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Tòa nhà (Building) gửi xe trong hệ thống PBMS.
/// Quản lý thông tin địa chỉ và các tầng (Floor) bên trong tòa nhà.
/// Tham chiếu SRS: §8.3.3, Table 3.5 — Physical Model: building
/// </summary>
public class Building : BaseEntity
{
    /// <summary>
    /// Mã tòa nhà (Ví dụ: "BLD-01", "EAST-WING").
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(20).
    /// </summary>

    /// <summary>
    /// Tên tòa nhà (Ví dụ: "Tòa nhà A", "Bãi xe Miền Đông").
    /// Độ dài tối đa: 50 ký tự.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Địa chỉ vật lý của tòa nhà.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Tổng số tầng của tòa nhà.
    /// </summary>
    public int TotalFloor { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tòa nhà (Ví dụ: Available, OutOfService).
    /// </summary>
    public BuildingStatus Status { get; set; } = BuildingStatus.Available;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Danh sách các tầng (Floor) thuộc tòa nhà này.
    /// </summary>
    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

    /// <summary>
    /// Danh sách các đặt chỗ (Booking) thuộc tòa nhà này.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// Danh sách các đăng ký vé tháng (MonthlySubscription) thuộc tòa nhà này.
    /// </summary>
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();

    /// <summary>
    /// Danh sách các thống kê doanh thu (RevenueStatistic) liên quan đến tòa nhà này.
    /// </summary>
}

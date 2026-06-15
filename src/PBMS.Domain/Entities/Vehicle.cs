namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Phương tiện (Vehicle) lưu trữ thông tin của xe trong hệ thống.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class Vehicle : BaseEntity
{
    public const string StatusActive = "ACTIVE";
    public const string StatusInactive = "INACTIVE";
    public const string StatusPending = "PENDING";
    public const string StatusSuspended = "SUSPENDED";
    public const string StatusArchived = "ARCHIVED";

    /// <summary>
    /// Khóa ngoại liên kết tới tài khoản sở hữu (Account).
    /// Có thể Null trong trường hợp khách vãng lai hoặc nhập biển số thô trước.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện (VehicleType - Xe máy, Ô tô).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Biển số xe độc nhất.
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(20).
    /// </summary>
    public string LicensePlate { get; set; } = null!;

    /// <summary>
    /// Biển số đã chuẩn hóa để kiểm tra trùng toàn hệ thống.
    /// </summary>
    /// <summary>
    /// Ngày phương tiện được đăng ký vào hệ thống bãi đỗ xe.
    /// </summary>
    public DateTime? RegisteredDay { get; set; }

    /// <summary>
    /// Trạng thái của xe trên hệ thống (Ví dụ: "Active", "Inactive", "Pending", "Suspended").
    /// </summary>
    public string VehicleStatus { get; set; } = StatusActive;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tài khoản (Account) sở hữu xe này.
    /// </summary>
    public virtual Account? Account { get; set; }

    /// <summary>
    /// Thông tin loại phương tiện (VehicleType) của xe.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Danh sách các lượt gửi xe (ParkingSession) của phương tiện này.
    /// </summary>
    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    /// <summary>
    /// Danh sách các đặt chỗ (Booking) của phương tiện này.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// Danh sách các vé tháng (MonthlySubscription) được đăng ký cho phương tiện này.
    /// </summary>
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();

    /// <summary>
    /// Danh sách các lịch sử sự cố hoặc vi phạm (Blacklist) liên quan đến phương tiện này.
    /// </summary>
    public virtual ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();
}

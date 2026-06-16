namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể tài khoản (Account) đại diện cho thông tin người dùng trong hệ thống PBMS.
/// Kế thừa từ BaseEntity (chứa Id, CreatedDate, UpdatedDate, v.v.).
/// </summary>
public class Account : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới bảng vai trò (Role).
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Tên đăng nhập độc nhất của người dùng.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Mật khẩu đã được mã hóa một chiều sử dụng thuật toán BCrypt.
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Họ và tên đầy đủ của chủ tài khoản.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Địa chỉ email đăng ký tài khoản (dùng để đăng nhập và khôi phục mật khẩu).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Số điện thoại liên lạc của chủ tài khoản.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản (Ví dụ: "Active", "Blocked", "Inactive").
    /// Mặc định là "Active" khi tạo mới.
    /// </summary>
    public string AccountStatus { get; set; } = "Active";

    /// <summary>
    /// Thuộc tính logic tự động xác định tài khoản có đang ở trạng thái hoạt động bình thường hay không.
    /// </summary>
    public bool IsActive => AccountStatus == "Active";

    /// <summary>
    /// Thông tin vai trò (Role) liên kết trực tiếp với tài khoản này.
    /// Từ khóa 'virtual' cho phép Entity Framework Core sử dụng cơ chế Lazy Loading (Tải chậm).
    /// Khi gọi 'account.Role', EF Core sẽ tự động truy vấn DB để lấy thông tin Role liên quan nếu chưa được load sẵn.
    /// </summary>
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Danh sách các phương tiện (Vehicle) sở hữu bởi tài khoản này.
    /// </summary>
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    /// <summary>
    /// Danh sách các lượt đặt chỗ (Booking) của tài khoản này.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// Danh sách các đăng ký vé tháng (MonthlySubscription) của tài khoản này.
    /// </summary>
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();

    /// <summary>
    /// Danh sách các nhật ký thao tác (AuditLog) thực hiện bởi tài khoản này.
    /// </summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Danh sách các thông báo (Notification) của tài khoản này.
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
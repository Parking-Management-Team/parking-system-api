namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Thông báo (Notification) được gửi tới người dùng/tài khoản trong hệ thống.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.23 — Physical Model: notification
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tài khoản nhận thông báo (Account).
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Tiêu đề của thông báo (Ví dụ: "Thanh toán thành công").
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung chi tiết của thông báo (Ví dụ: "Hợp đồng vé tháng của bạn đã được kích hoạt thành công.").
    /// </summary>
    public string Message { get; set; } = null!;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tài khoản nhận thông báo.
    /// </summary>
    public virtual Account Account { get; set; } = null!;
}

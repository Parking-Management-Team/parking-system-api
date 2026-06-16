namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Nhật ký thao tác (AuditLog) để truy vết và lưu lại hoạt động của người dùng hoặc hệ thống.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.24 — Physical Model: audit_log
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tài khoản thực hiện hành động (Account).
    /// Có thể Null nếu hành động do hệ thống tự động chạy (System-generated logs).
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Tên hành động hoặc thao tác thực hiện (Ví dụ: "CREATE", "UPDATE", "DELETE", "LOGIN").
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Bảng hoặc thực thể bị tác động bởi hành động này (Ví dụ: "vehicle", "pricing_policy").
    /// </summary>
    public string? TargetTable { get; set; }

    /// <summary>
    /// ID của bản ghi bị tác động trong bảng TargetTable.
    /// </summary>
    public int? TargetId { get; set; }

    /// <summary>
    /// Mô tả chi tiết hành động hoặc nội dung cụ thể (Ví dụ: "Tạo mới xe với biển số 51F-123.45").
    /// </summary>
    public string? Description { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tài khoản thực hiện hành động.
    /// </summary>
    public virtual Account? Account { get; set; }
}

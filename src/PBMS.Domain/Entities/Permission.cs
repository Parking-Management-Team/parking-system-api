namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Quyền hạn (Permission) lưu trữ các quyền chức năng trong hệ thống.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Mã quyền duy nhất (Ví dụ: "CREATE_USER", "VIEW_REPORT").
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(50).
    /// </summary>
    public string PermissionCode { get; set; } = null!;

    /// <summary>
    /// Tên quyền hiển thị (Ví dụ: "Tạo người dùng", "Xem báo cáo").
    /// </summary>
    public string PermissionName { get; set; } = null!;

    /// <summary>
    /// Mô tả chi tiết về chức năng của quyền này.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái quyền (Ví dụ: "Active", "Inactive").
    /// </summary>
    public string PermissionStatus { get; set; } = "Active";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Danh sách liên kết Nhiều-Nhiều thông qua bảng trung gian RolePermission.
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

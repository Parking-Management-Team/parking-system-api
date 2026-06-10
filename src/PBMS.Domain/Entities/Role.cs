namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Vai trò (Role) xác định các quyền hạn của tài khoản trong hệ thống PBMS.
/// Kế thừa từ BaseEntity (chứa Id, CreatedDate, UpdatedDate, v.v.).
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Tên của vai trò (Ví dụ: "Admin", "Manager", "Staff", "Customer").
    /// </summary>
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// Mô tả chi tiết về chức năng hoặc quyền hạn của vai trò này.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Danh sách các tài khoản đang thuộc vai trò này (Quan hệ một - nhiều).
    /// Từ khóa 'virtual' cho phép Entity Framework Core sử dụng cơ chế Lazy Loading (Tải chậm).
    /// Khi gọi 'role.Accounts', EF Core sẽ tự động truy vấn DB để lấy danh sách Account liên quan nếu chưa được load sẵn.
    /// </summary>
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    /// <summary>
    /// Danh sách liên kết Nhiều-Nhiều thông qua bảng trung gian RolePermission.
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
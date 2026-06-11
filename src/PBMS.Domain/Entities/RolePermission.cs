namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể liên kết trung gian Nhiều-Nhiều (Many-to-Many Join Entity) giữa Role và Permission.
/// Không kế thừa từ BaseEntity vì sử dụng khóa chính hợp phần (Composite PK) là role_id và permission_id.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Khóa ngoại liên kết tới vai trò (Role). Part of Composite PK.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới quyền hạn (Permission). Part of Composite PK.
    /// </summary>
    public int PermissionId { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin vai trò (Role).
    /// </summary>
    public virtual Role Role { get; set; } = null!;

    /// <summary>
    /// Thông tin quyền hạn (Permission).
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}

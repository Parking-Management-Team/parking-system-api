
using System.ComponentModel.DataAnnotations;

namespace PBMS.Domain.Entities;

/// <summary>
/// Lớp cơ sở (Base Class) cho tất cả các thực thể trong hệ thống.
/// Chứa các thuộc tính chung: Id, CreatedAt, RowVersion.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa chính tự tăng, định danh duy nhất của bản ghi.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi, mặc định là thời gian UTC hiện tại.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Dùng để kiểm soát xung đột đồng thời (Concurrency Control).
    /// Trên PostgreSQL, thuộc tính uint kết hợp với [Timestamp] sẽ tự động ánh xạ tới cột hệ thống 'xmin'.
    /// </summary>
    [Timestamp]
    public uint RowVersion { get; set; }
}
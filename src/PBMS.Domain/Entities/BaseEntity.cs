
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
    /// EF Core sẽ tự động kiểm tra giá trị này khi Update/Delete:
    /// - Nếu RowVersion trong DB khác với RowVersion client gửi lên → ném DbUpdateConcurrencyException.
    /// - Giá trị này tự động tăng mỗi khi bản ghi được cập nhật (do cơ chế Timestamp/RowVersion của database).
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
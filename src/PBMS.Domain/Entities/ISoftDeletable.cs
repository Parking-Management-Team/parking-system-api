using System;

namespace PBMS.Domain.Entities;

/// <summary>
/// Interface cho các thực thể hỗ trợ xóa mềm (Soft Delete).
/// Giúp giữ lại dữ liệu trong database phục vụ mục đích Audit và đối soát.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Đánh dấu bản ghi đã bị xóa hay chưa.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Thời điểm thực hiện hành động xóa.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// ID của người dùng (Account.Id) thực hiện hành động xóa.
    /// </summary>
    int? DeletedBy { get; set; }
}

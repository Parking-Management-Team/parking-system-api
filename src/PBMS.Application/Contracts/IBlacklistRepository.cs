using PBMS.Application.Common;
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho thực thể Blacklist.
/// </summary>
public interface IBlacklistRepository : IRepository<PBMS.Domain.Entities.Blacklist>
{
    /// <summary>
    /// Lấy chi tiết bản ghi chặn kèm thông tin Vehicle và Card.
    /// </summary>
    Task<PBMS.Domain.Entities.Blacklist?> GetBlacklistWithDetailsAsync(int id);

    /// <summary>
    /// Lấy danh sách đen có phân trang kèm thông tin liên quan.
    /// </summary>
    Task<PagedResult<PBMS.Domain.Entities.Blacklist>> GetPagedBlacklistWithDetailsAsync(int pageIndex, int pageSize);
}

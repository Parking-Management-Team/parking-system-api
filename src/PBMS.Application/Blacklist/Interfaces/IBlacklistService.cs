using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Common;

namespace PBMS.Application.Blacklist.Interfaces;

/// <summary>
/// Giao diện dịch vụ quản lý danh sách đen (Blacklist).
/// </summary>
public interface IBlacklistService
{
    /// <summary>
    /// Thêm một xe/thẻ vào danh sách đen.
    /// </summary>
    Task<BlacklistDto> AddToBlacklistAsync(AddToBlacklistRequest request);

    /// <summary>
    /// Xóa một mục khỏi danh sách đen (Gỡ chặn).
    /// </summary>
    Task RemoveFromBlacklistAsync(int id);

    /// <summary>
    /// Lấy chi tiết một bản ghi chặn.
    /// </summary>
    Task<BlacklistDto> GetBlacklistByIdAsync(int id);

    /// <summary>
    /// Lấy danh sách đen có phân trang.
    /// </summary>
    Task<PagedResult<BlacklistDto>> GetBlacklistPagedAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Kiểm tra xem một xe có đang bị chặn không.
    /// </summary>
    Task<bool> IsVehicleBlockedAsync(int vehicleId);

    /// <summary>
    /// Kiểm tra xem một thẻ có đang bị chặn không.
    /// </summary>
    Task<bool> IsCardBlockedAsync(int cardId);
}

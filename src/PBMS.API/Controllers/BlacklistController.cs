using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Blacklist.Interfaces;
using PBMS.Application.Common;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý danh sách đen (Blacklist).
/// Cho phép chặn xe, thẻ hoặc từ sự cố.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BlacklistController : ControllerBase
{
    private readonly IBlacklistService _blacklistService;

    public BlacklistController(IBlacklistService blacklistService)
    {
        _blacklistService = blacklistService;
    }

    /// <summary>
    /// Thêm một thực thể vào danh sách đen.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToBlacklist([FromBody] AddToBlacklistRequest request)
    {
        var result = await _blacklistService.AddToBlacklistAsync(request);
        return Ok(BaseResponse<BlacklistDto>.Ok(result, "Entity has been added to blacklist successfully."));
    }

    /// <summary>
    /// Gỡ bỏ một thực thể khỏi danh sách đen.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromBlacklist(int id)
    {
        await _blacklistService.RemoveFromBlacklistAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Entity has been removed from blacklist successfully."));
    }

    /// <summary>
    /// Lấy chi tiết một bản ghi chặn.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlacklistById(int id)
    {
        var result = await _blacklistService.GetBlacklistByIdAsync(id);
        return Ok(BaseResponse<BlacklistDto>.Ok(result));
    }

    /// <summary>
    /// Lấy danh sách đen có phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBlacklistPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _blacklistService.GetBlacklistPagedAsync(pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<BlacklistDto>>.Ok(result));
    }

    /// <summary>
    /// Kiểm tra xem xe có bị chặn không.
    /// </summary>
    [HttpGet("check-vehicle/{vehicleId}")]
    public async Task<IActionResult> IsVehicleBlocked(int vehicleId)
    {
        var result = await _blacklistService.IsVehicleBlockedAsync(vehicleId);
        return Ok(BaseResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Kiểm tra xem thẻ có bị chặn không.
    /// </summary>
    [HttpGet("check-card/{cardId}")]
    public async Task<IActionResult> IsCardBlocked(int cardId)
    {
        var result = await _blacklistService.IsCardBlockedAsync(cardId);
        return Ok(BaseResponse<bool>.Ok(result));
    }
}

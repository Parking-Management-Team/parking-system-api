using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý cấu hình giá vé tháng (SubscriptionPriceConfig).
/// </summary>
[ApiController]
[Route("api/subscription-price-configs")]
public class SubscriptionPriceConfigsController : ControllerBase
{
    private readonly ISubscriptionPriceConfigService _service;

    public SubscriptionPriceConfigsController(ISubscriptionPriceConfigService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách cấu hình giá vé tháng (hỗ trợ lọc theo loại xe và trạng thái hoạt động).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? vehicleTypeId, [FromQuery] bool? onlyActive)
    {
        var result = await _service.GetAllConfigsAsync(vehicleTypeId, onlyActive);
        return Ok(BaseResponse<IEnumerable<SubscriptionPriceConfigDto>>.Ok(result));
    }

    /// <summary>
    /// Lấy cấu hình giá vé tháng đang hoạt động của một loại xe.
    /// </summary>
    [HttpGet("active/{vehicleTypeId}")]
    public async Task<IActionResult> GetActive(int vehicleTypeId)
    {
        var result = await _service.GetActiveConfigByVehicleTypeAsync(vehicleTypeId);
        if (result == null)
        {
            return NotFound(BaseResponse<SubscriptionPriceConfigDto>.Fail("NOT_FOUND", $"No active subscription price configuration found for vehicle type ID: {vehicleTypeId}"));
        }
        return Ok(BaseResponse<SubscriptionPriceConfigDto>.Ok(result));
    }

    /// <summary>
    /// Tạo cấu hình giá vé tháng mới (Tự động vô hiệu hóa giá cũ).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionPriceConfigRequest request)
    {
        var result = await _service.CreateConfigAsync(request);
        return CreatedAtAction(nameof(GetActive), new { vehicleTypeId = result.VehicleTypeId }, BaseResponse<SubscriptionPriceConfigDto>.Ok(result, "Subscription price configuration created successfully."));
    }

    /// <summary>
    /// Vô hiệu hóa (Inactive) cấu hình giá vé tháng.
    /// </summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var success = await _service.DeactivateConfigAsync(id);
        if (!success)
        {
            return BadRequest(BaseResponse<string>.Fail("BAD_REQUEST", "Configuration is already inactive or cannot be updated."));
        }
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Subscription price configuration deactivated successfully."));
    }

    /// <summary>
    /// Xóa mềm (Soft Delete) cấu hình giá vé tháng.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteConfigAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Subscription price configuration soft deleted successfully."));
    }
}

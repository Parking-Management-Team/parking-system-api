using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Engine;
using System;
using System.Threading.Tasks;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller quản lý và tính toán thử nghiệm Pricing Engine (Rule Engine).
/// </summary>
[ApiController]
[Route("api/pricing-engine")]
[Authorize]
public class PricingEngineController : ControllerBase
{
    private readonly IPricingCalculationService _pricingCalculationService;

    public PricingEngineController(IPricingCalculationService pricingCalculationService)
    {
        _pricingCalculationService = pricingCalculationService 
            ?? throw new ArgumentNullException(nameof(pricingCalculationService));
    }

    /// <summary>
    /// Thử nghiệm tính toán phí gửi xe bằng Rule-based Engine mới.
    /// Không lưu log đối soát.
    ///
    /// Route  : GET /api/pricing-engine/calculate
    /// </summary>
    [HttpGet("calculate")]
    public async Task<ActionResult<BaseResponse<PricingResult>>> Calculate(
        [FromQuery] int vehicleTypeId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var result = await _pricingCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut);
        return Ok(BaseResponse<PricingResult>.Ok(result, "Calculated pricing successfully via Rule Engine."));
    }

    /// <summary>
    /// Tính toán phí gửi xe bằng Rule-based Engine mới và lưu log audit.
    ///
    /// Route  : POST /api/pricing-engine/calculate-and-log
    /// </summary>
    [HttpPost("calculate-and-log")]
    public async Task<ActionResult<BaseResponse<PricingResult>>> CalculateAndLog(
        [FromQuery] int vehicleTypeId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int? bookingId = null,
        [FromQuery] int? parkingSessionId = null)
    {
        var result = await _pricingCalculationService.CalculateFeeAndLogAsync(vehicleTypeId, checkIn, checkOut, bookingId, parkingSessionId);
        return Ok(BaseResponse<PricingResult>.Ok(result, "Calculated and logged pricing successfully via Rule Engine."));
    }
}

using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller quản lý Chính sách giá (Pricing Policy Management).
/// Endpoint chính: /api/pricing-policies
///
/// Actor: Parking Manager
///
/// Các chức năng (PBMS-86 — PS06: PRICING CONFIG):
///   POST   /api/pricing-policies                            → Tạo mới chính sách giá (Scenario 1)
///   GET    /api/pricing-policies                            → Lấy danh sách chính sách giá
///   GET    /api/pricing-policies/{id}                       → Lấy chi tiết chính sách giá theo ID
///   PUT    /api/pricing-policies/{id}                       → Cập nhật chính sách giá
///   POST   /api/pricing-policies/{id}/windows               → Thêm khung giờ vào chính sách
///   PUT    /api/pricing-policies/windows/{windowId}         → Cập nhật khung giờ
///   DELETE /api/pricing-policies/windows/{windowId}         → Xóa khung giờ
/// </summary>
[ApiController]
[Route("api/pricing-policies")]
public class PricingPoliciesController : ControllerBase
{
    private readonly IPricingPolicyService _pricingPolicyService;

    /// <summary>
    /// Constructor nhận IPricingPolicyService qua Dependency Injection.
    /// </summary>
    public PricingPoliciesController(IPricingPolicyService pricingPolicyService)
    {
        _pricingPolicyService = pricingPolicyService
            ?? throw new ArgumentNullException(nameof(pricingPolicyService));
    }

    // -----------------------------------------------------------------------
    // POST /api/pricing-policies — [Scenario 1] Tạo chính sách giá mới
    // -----------------------------------------------------------------------

    /// <summary>
    /// [Scenario 1 — Bước 1/2] Tạo mới một Chính sách giá kèm danh sách Khung giờ.
    /// Policy được tạo với trạng thái INACTIVE.
    /// Sau khi tạo, gọi POST /{id}/activate để kích hoạt.
    ///
    /// Route  : POST /api/pricing-policies
    /// Body   : CreatePricingPolicyRequest (VehicleTypeId, PolicyName, PricingWindows)
    /// Returns: 201 Created + PricingPolicyDto (status=INACTIVE) nếu thành công
    ///          400 Bad Request nếu tham số không hợp lệ
    ///          404 Not Found nếu VehicleTypeId không tồn tại
    ///          422 Unprocessable nếu vi phạm business rule
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<PricingPolicyDto>>> CreatePricingPolicy(
        [FromBody] CreatePricingPolicyRequest request)
    {
        var policy = await _pricingPolicyService.CreatePricingPolicyAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetPricingPolicyById),
            routeValues: new { id = policy.Id },
            value: BaseResponse<PricingPolicyDto>.Ok(policy, "Tạo chính sách giá thành công. Hệ thống đang ở trạng thái Inactive. Vui lòng kích hoạt (activate) để áp dụng.")
        );
    }

    // -----------------------------------------------------------------------
    // POST /api/pricing-policies/{id}/activate — [Scenario 1 - Bước 2] Kích hoạt chính sách
    // -----------------------------------------------------------------------

    /// <summary>
    /// [Scenario 1 — Bước 2/2] Kích hoạt Chính sách giá từ INACTIVE → ACTIVE.
    ///
    /// Validate trước khi kích hoạt:
    ///   - Các PricingWindow phủ đủ 24h và không overlap (BR-FEE-027/028).
    ///   - Không overlap với chính sách Active/Inactive khác cùng loại xe (BR-FEE-025).
    ///
    /// Route  : POST /api/pricing-policies/{id}/activate
    /// Returns: 200 OK + PricingPolicyDto (status=Active) nếu thành công
    ///          404 Not Found nếu không tìm thấy chính sách
    ///          422 Unprocessable nếu vi phạm business rule (overlap, 24h coverage, ...)
    /// </summary>
    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<BaseResponse<PricingPolicyDto>>> ActivatePricingPolicy(int id)
    {
        var policy = await _pricingPolicyService.ActivatePricingPolicyAsync(id);

        return Ok(BaseResponse<PricingPolicyDto>.Ok(policy, "Kích hoạt chính sách giá thành công. Hệ thống đã áp dụng bảng giá mới vào bộ quy tắc tính phí."));
    }

    // -----------------------------------------------------------------------
    // GET /api/pricing-policies — Lấy danh sách chính sách giá
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách tất cả Chính sách giá (kèm danh sách khung giờ).
    /// Hỗ trợ lọc theo loại xe và trạng thái.
    ///
    /// Route  : GET /api/pricing-policies?vehicleTypeId=1&amp;status=Active
    /// Returns: 200 OK + danh sách PricingPolicyDto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<IEnumerable<PricingPolicyDto>>>> GetAllPricingPolicies(
        [FromQuery] int? vehicleTypeId = null,
        [FromQuery] string? status = null)
    {
        var policies = await _pricingPolicyService.GetAllPricingPoliciesAsync(vehicleTypeId, status);

        return Ok(BaseResponse<IEnumerable<PricingPolicyDto>>.Ok(policies));
    }

    // -----------------------------------------------------------------------
    // GET /api/pricing-policies/{id} — Lấy chi tiết chính sách giá
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy thông tin chi tiết một Chính sách giá theo ID (kèm danh sách khung giờ).
    ///
    /// Route  : GET /api/pricing-policies/{id}
    /// Returns: 200 OK + PricingPolicyDto nếu tìm thấy
    ///          404 Not Found nếu không có chính sách với ID này
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<PricingPolicyDto>>> GetPricingPolicyById(int id)
    {
        var policy = await _pricingPolicyService.GetPricingPolicyByIdAsync(id);

        return Ok(BaseResponse<PricingPolicyDto>.Ok(policy));
    }

    // -----------------------------------------------------------------------
    // PUT /api/pricing-policies/{id} — Cập nhật chính sách giá
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thông tin Chính sách giá (PolicyName, EffectiveEnd, Status).
    /// Hỗ trợ partial update: chỉ cập nhật các trường được cung cấp.
    ///
    /// Route  : PUT /api/pricing-policies/{id}
    /// Body   : UpdatePricingPolicyRequest
    /// Returns: 200 OK + PricingPolicyDto sau khi cập nhật
    ///          404 Not Found nếu không tìm thấy chính sách
    ///          400 Bad Request nếu tham số không hợp lệ
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<PricingPolicyDto>>> UpdatePricingPolicy(
        int id,
        [FromBody] UpdatePricingPolicyRequest request)
    {
        var policy = await _pricingPolicyService.UpdatePricingPolicyAsync(id, request);

        return Ok(BaseResponse<PricingPolicyDto>.Ok(policy, "Cập nhật chính sách giá thành công."));
    }

    // -----------------------------------------------------------------------
    // POST /api/pricing-policies/{id}/windows — Thêm khung giờ vào chính sách
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thêm một Khung giờ tính giá mới vào Chính sách giá đã tồn tại.
    ///
    /// Route  : POST /api/pricing-policies/{id}/windows
    /// Body   : CreatePricingWindowRequest
    /// Returns: 201 Created + PricingWindowDto nếu thành công
    ///          404 Not Found nếu không tìm thấy chính sách
    ///          400 Bad Request nếu tham số không hợp lệ
    /// </summary>
    [HttpPost("{id:int}/windows")]
    public async Task<ActionResult<BaseResponse<PricingWindowDto>>> AddPricingWindow(
        int id,
        [FromBody] CreatePricingWindowRequest request)
    {
        var window = await _pricingPolicyService.AddPricingWindowAsync(id, request);

        return CreatedAtAction(
            actionName: nameof(GetPricingPolicyById),
            routeValues: new { id },
            value: BaseResponse<PricingWindowDto>.Ok(window, "Thêm khung giờ tính giá thành công.")
        );
    }

    // -----------------------------------------------------------------------
    // PUT /api/pricing-policies/windows/{windowId} — Cập nhật khung giờ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thông tin một Khung giờ tính giá.
    /// Hỗ trợ partial update: chỉ cập nhật các trường được cung cấp.
    ///
    /// Route  : PUT /api/pricing-policies/windows/{windowId}
    /// Body   : UpdatePricingWindowRequest
    /// Returns: 200 OK + PricingWindowDto sau khi cập nhật
    ///          404 Not Found nếu không tìm thấy khung giờ
    ///          400 Bad Request nếu tham số không hợp lệ
    /// </summary>
    [HttpPut("windows/{windowId:int}")]
    public async Task<ActionResult<BaseResponse<PricingWindowDto>>> UpdatePricingWindow(
        int windowId,
        [FromBody] UpdatePricingWindowRequest request)
    {
        var window = await _pricingPolicyService.UpdatePricingWindowAsync(windowId, request);

        return Ok(BaseResponse<PricingWindowDto>.Ok(window, "Cập nhật khung giờ tính giá thành công."));
    }

    // -----------------------------------------------------------------------
    // DELETE /api/pricing-policies/windows/{windowId} — Xóa khung giờ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Xóa một Khung giờ tính giá.
    /// Không cho phép xóa nếu đây là khung giờ cuối cùng của chính sách.
    ///
    /// Route  : DELETE /api/pricing-policies/windows/{windowId}
    /// Returns: 204 No Content nếu xóa thành công
    ///          404 Not Found nếu không tìm thấy khung giờ
    ///          409 Conflict nếu là khung giờ cuối cùng
    /// </summary>
    [HttpDelete("windows/{windowId:int}")]
    public async Task<ActionResult> DeletePricingWindow(int windowId)
    {
        await _pricingPolicyService.DeletePricingWindowAsync(windowId);

        return NoContent();
    }
}

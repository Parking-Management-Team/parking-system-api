using PBMS.Application.Pricing.DTOs;

namespace PBMS.Application.Pricing.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Chính sách giá (PricingPolicy Management).
///
/// Các chức năng theo Acceptance Criteria PBMS-86:
///   Scenario 1 — CreatePricingPolicyAsync   : Tạo chính sách giá mới (status = INACTIVE)
///              — ActivatePricingPolicyAsync  : Kích hoạt chính sách (validate 24h + no-overlap → ACTIVE)
///   GetByIdAsync / GetAllAsync              : Truy vấn chính sách giá
///   UpdatePricingPolicyAsync                : Chỉnh sửa chính sách giá (bị khóa khi ACTIVE)
///   AddWindowAsync / UpdateWindowAsync      : Quản lý khung giờ tính giá (bị khóa khi ACTIVE)
/// </summary>
public interface IPricingPolicyService
{
    /// <summary>
    /// [Scenario 1 — Bước 1/2] Tạo mới một Chính sách giá kèm danh sách Khung giờ.
    /// Policy được tạo với trạng thái INACTIVE; cần gọi ActivatePricingPolicyAsync để kích hoạt.
    ///
    /// Nghiệp vụ:
    ///   1. Kiểm tra VehicleTypeId tồn tại.
    ///   2. Validate: EffectiveEnd (nếu có) > EffectiveStart.
    ///   3. Validate mỗi PricingWindow params (BaseDuration, BasePrice, IncrementBlock, WindowCap).
    ///   4. Tạo PricingPolicy (status=INACTIVE) + toàn bộ PricingWindows → lưu DB.
    ///   5. Trả về PricingPolicyDto đã tạo (trạng thái INACTIVE).
    ///
    /// Lỗi có thể xảy ra:
    ///   - VehicleTypeId không tồn tại → DomainException "VEHICLE_TYPE_NOT_FOUND"
    ///   - EffectiveEnd &lt;= EffectiveStart → DomainException "INVALID_EFFECTIVE_DATE_RANGE"
    ///   - Danh sách PricingWindows rỗng → DomainException "PRICING_WINDOWS_REQUIRED"
    ///   - WindowCap &lt; BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingPolicyDto> CreatePricingPolicyAsync(CreatePricingPolicyRequest request);

    /// <summary>
    /// [Scenario 1 — Bước 2/2] Kích hoạt Chính sách giá từ INACTIVE → ACTIVE.
    ///
    /// Nghiệp vụ:
    ///   1. Validate các PricingWindow phủ đủ 24h và không overlap nhau (BR-FEE-027/028).
    ///   2. Validate không có Policy nào cùng VehicleType overlap thời gian hiệu lực (BR-FEE-025).
    ///   3. Chuyển trạng thái policy sang ACTIVE → lưu DB.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Policy không tồn tại → DomainException "PRICING_POLICY_NOT_FOUND"
    ///   - Policy đã ACTIVE hoặc EXPIRED → DomainException "POLICY_ALREADY_ACTIVE_OR_EXPIRED"
    ///   - Windows không phủ đủ 24h → DomainException "WINDOWS_DO_NOT_COVER_24_HOURS"
    ///   - Windows overlap nhau → DomainException "WINDOWS_OVERLAP"
    ///   - Policy overlap với policy khác cùng VehicleType → DomainException "POLICY_OVERLAP"
    /// </summary>
    Task<PricingPolicyDto> ActivatePricingPolicyAsync(int id);

    /// <summary>
    /// Lấy thông tin Chính sách giá theo ID (kèm danh sách PricingWindow).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy → DomainException "PRICING_POLICY_NOT_FOUND"
    /// </summary>
    Task<PricingPolicyDto> GetPricingPolicyByIdAsync(int id);

    /// <summary>
    /// Lấy danh sách tất cả Chính sách giá (kèm danh sách PricingWindow).
    /// Hỗ trợ lọc theo VehicleTypeId và trạng thái.
    /// </summary>
    Task<IEnumerable<PricingPolicyDto>> GetAllPricingPoliciesAsync(int? vehicleTypeId = null, string? status = null);

    /// <summary>
    /// Cập nhật thông tin Chính sách giá (partial update: chỉ cập nhật các trường có giá trị).
    ///
    /// Bị khóa khi Policy đã ACTIVE (BR-FEE-029):
    ///   - Không được sửa VehicleTypeId, PricingWindow, hoặc bất kỳ tham số ảnh hưởng kết quả tính phí.
    ///   - Chỉ cho phép sửa PolicyName, EffectiveEnd (theo BR-FEE-032), PricingPolicyStatus.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy → DomainException "PRICING_POLICY_NOT_FOUND"
    ///   - Cố sửa effective_start khi ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY_START_DATE"
    ///   - EffectiveEnd &lt;= EffectiveStart → DomainException "INVALID_EFFECTIVE_DATE_RANGE"
    ///   - Status không hợp lệ → DomainException "INVALID_PRICING_POLICY_STATUS"
    ///   - Policy EXPIRED cố sửa EffectiveEnd → DomainException "CANNOT_MODIFY_EXPIRED_POLICY"
    /// </summary>
    Task<PricingPolicyDto> UpdatePricingPolicyAsync(int id, UpdatePricingPolicyRequest request);

    /// <summary>
    /// Thêm một Khung giờ mới vào Chính sách giá đã tồn tại.
    /// Bị khóa khi Policy đã ACTIVE (BR-FEE-029).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Chính sách không tồn tại → DomainException "PRICING_POLICY_NOT_FOUND"
    ///   - Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY"
    ///   - WindowCap &lt; BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingWindowDto> AddPricingWindowAsync(int pricingPolicyId, CreatePricingWindowRequest request);

    /// <summary>
    /// Cập nhật một Khung giờ tính giá cụ thể (partial update).
    /// Bị khóa khi Policy đã ACTIVE (BR-FEE-029).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Khung giờ không tồn tại → DomainException "PRICING_WINDOW_NOT_FOUND"
    ///   - Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY"
    ///   - WindowCap &lt; BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingWindowDto> UpdatePricingWindowAsync(int pricingWindowId, UpdatePricingWindowRequest request);

    /// <summary>
    /// Xóa một Khung giờ tính giá. Không cho phép xóa nếu chính sách chỉ còn 1 window.
    /// Bị khóa khi Policy đã ACTIVE (BR-FEE-029).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Khung giờ không tồn tại → DomainException "PRICING_WINDOW_NOT_FOUND"
    ///   - Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY"
    ///   - Chính sách chỉ còn 1 window → DomainException "CANNOT_DELETE_LAST_PRICING_WINDOW"
    /// </summary>
    Task DeletePricingWindowAsync(int pricingWindowId);
}

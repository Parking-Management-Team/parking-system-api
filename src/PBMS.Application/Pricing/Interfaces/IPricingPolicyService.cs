using PBMS.Application.Pricing.DTOs;
using PBMS.Domain.Entities;

namespace PBMS.Application.Pricing.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Chính sách giá (PricingPolicy Management).
///
/// Các chức năng theo Acceptance Criteria PBMS-86:
///   Scenario 1 — CreatePricingPolicyAsync   : Tạo và kích hoạt chính sách giá mới
///   GetByIdAsync / GetAllAsync              : Truy vấn chính sách giá
///   UpdatePricingPolicyAsync                : Chỉnh sửa chính sách giá
///   AddWindowAsync / UpdateWindowAsync      : Quản lý khung giờ tính giá
/// </summary>
public interface IPricingPolicyService
{
    /// <summary>
    /// [Scenario 1] Tạo mới một Chính sách giá kèm danh sách Khung giờ.
    ///
    /// Nghiệp vụ:
    ///   1. Kiểm tra VehicleTypeId tồn tại.
    ///   2. Kiểm tra không có chính sách Active khác cho cùng VehicleType giao thời gian (nếu áp dụng).
    ///   3. Validate: EffectiveEnd (nếu có) > EffectiveStart.
    ///   4. Validate mỗi PricingWindow: BaseDurationMinutes > 0, IncrementBlockMinutes > 0,
    ///      WindowCap >= BasePrice (nếu có).
    ///   5. Tạo PricingPolicy + toàn bộ PricingWindows → lưu DB.
    ///   6. Trả về PricingPolicyDto đã tạo (trạng thái Active theo mặc định).
    ///
    /// Lỗi có thể xảy ra:
    ///   - VehicleTypeId không tồn tại → DomainException "VEHICLE_TYPE_NOT_FOUND"
    ///   - EffectiveEnd <= EffectiveStart → DomainException "INVALID_EFFECTIVE_DATE_RANGE"
    ///   - Danh sách PricingWindows rỗng → DomainException "PRICING_WINDOWS_REQUIRED"
    ///   - WindowCap < BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingPolicyDto> CreatePricingPolicyAsync(CreatePricingPolicyRequest request);

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
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy → DomainException "PRICING_POLICY_NOT_FOUND"
    ///   - EffectiveEnd <= EffectiveStart → DomainException "INVALID_EFFECTIVE_DATE_RANGE"
    /// </summary>
    Task<PricingPolicyDto> UpdatePricingPolicyAsync(int id, UpdatePricingPolicyRequest request);

    /// <summary>
    /// Thêm một Khung giờ mới vào Chính sách giá đã tồn tại.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Chính sách không tồn tại → DomainException "PRICING_POLICY_NOT_FOUND"
    ///   - WindowCap < BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingWindowDto> AddPricingWindowAsync(int pricingPolicyId, CreatePricingWindowRequest request);

    /// <summary>
    /// Cập nhật một Khung giờ tính giá cụ thể (partial update).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Khung giờ không tồn tại → DomainException "PRICING_WINDOW_NOT_FOUND"
    ///   - WindowCap < BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE"
    /// </summary>
    Task<PricingWindowDto> UpdatePricingWindowAsync(int pricingWindowId, UpdatePricingWindowRequest request);

    /// <summary>
    /// Xóa một Khung giờ tính giá. Không cho phép xóa nếu chính sách chỉ còn 1 window.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Khung giờ không tồn tại → DomainException "PRICING_WINDOW_NOT_FOUND"
    ///   - Chính sách chỉ còn 1 window → DomainException "CANNOT_DELETE_LAST_PRICING_WINDOW"
    /// </summary>
    Task DeletePricingWindowAsync(int pricingWindowId);
}

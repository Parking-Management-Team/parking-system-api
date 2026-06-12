using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

/// <summary>
/// Interface repository cho PricingPolicy và PricingWindow.
/// Mở rộng IRepository&lt;PricingPolicy&gt; với các truy vấn chuyên biệt cho nghiệp vụ tính giá.
/// </summary>
public interface IPricingPolicyRepository : IRepository<PricingPolicy>
{
    /// <summary>
    /// Lấy PricingPolicy theo ID, kèm eager load danh sách PricingWindows và VehicleType.
    /// </summary>
    Task<PricingPolicy?> GetByIdWithWindowsAsync(int id);

    /// <summary>
    /// Lấy toàn bộ danh sách PricingPolicy, kèm eager load PricingWindows và VehicleType.
    /// Hỗ trợ lọc theo vehicleTypeId và trạng thái.
    /// </summary>
    Task<IEnumerable<PricingPolicy>> GetAllWithWindowsAsync(int? vehicleTypeId = null, string? status = null);

    /// <summary>
    /// Tìm PricingPolicy đang Active (PricingPolicyStatus == "Active") cho loại xe cụ thể
    /// tại thời điểm cho trước. Dùng để tra cứu bảng giá khi tính phí check-out.
    /// </summary>
    /// <param name="vehicleTypeId">ID loại xe.</param>
    /// <param name="atTime">Thời điểm cần tra cứu (thường là thời điểm check-in hoặc check-out).</param>
    Task<PricingPolicy?> GetActivePolicyAsync(int vehicleTypeId, DateTime atTime);

    // ------------------------------------------------------------------
    // PricingWindow operations
    // ------------------------------------------------------------------

    /// <summary>
    /// Lấy PricingWindow theo ID, kèm thông tin PricingPolicy cha.
    /// </summary>
    Task<PricingWindow?> GetWindowByIdAsync(int pricingWindowId);

    /// <summary>
    /// Thêm một PricingWindow mới vào database.
    /// </summary>
    Task AddWindowAsync(PricingWindow window);

    /// <summary>
    /// Lưu thay đổi của PricingWindow (Update).
    /// </summary>
    void UpdateWindow(PricingWindow window);

    /// <summary>
    /// Xóa một PricingWindow khỏi database.
    /// </summary>
    Task RemoveWindowAsync(PricingWindow window);

    /// <summary>
    /// Đếm số PricingWindow của một PricingPolicy.
    /// Dùng để ngăn xóa window cuối cùng của chính sách (BR: phải có ít nhất 1 window).
    /// </summary>
    Task<int> CountWindowsByPolicyIdAsync(int pricingPolicyId);
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// Request body để tạo mới một Chính sách giá (PricingPolicy).
/// Bao gồm ít nhất một khung giờ (PricingWindow) trong danh sách.
/// </summary>
public class CreatePricingPolicyRequest
{
    /// <summary>
    /// ID loại phương tiện áp dụng chính sách giá.
    /// Bắt buộc phải chỉ định.
    /// </summary>
    [Required]
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Tên chính sách giá (tối đa 100 ký tự).
    /// Ví dụ: "Bảng giá vãng lai xe máy".
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PolicyName { get; set; } = null!;

    /// <summary>
    /// Ngày bắt đầu hiệu lực của chính sách.
    /// Hệ thống áp dụng ngay khi kích hoạt thành công (Scenario 1).
    /// </summary>
    [Required]
    public DateTime EffectiveStart { get; set; }

    /// <summary>
    /// Ngày kết thúc hiệu lực (tùy chọn, null nếu vô thời hạn).
    /// Nếu có giá trị, phải sau EffectiveStart.
    /// </summary>
    public DateTime? EffectiveEnd { get; set; }

    /// <summary>
    /// Danh sách khung giờ tính giá của chính sách này.
    /// Phải có ít nhất 1 khung giờ.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<CreatePricingWindowRequest> PricingWindows { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// Request body để cập nhật thông tin một Chính sách giá (PricingPolicy).
/// Tất cả các trường đều tùy chọn (partial update).
/// </summary>
public class UpdatePricingPolicyRequest
{
    /// <summary>
    /// Tên chính sách giá mới (tối đa 100 ký tự, null nếu không thay đổi).
    /// </summary>
    [MaxLength(100)]
    public string? PolicyName { get; set; }

    /// <summary>
    /// Ngày bắt đầu hiệu lực mới (null nếu không thay đổi).
    /// </summary>
    public DateTime? EffectiveStart { get; set; }

    /// <summary>
    /// Ngày kết thúc hiệu lực mới (null nếu không thay đổi hoặc vô thời hạn).
    /// </summary>
    public DateTime? EffectiveEnd { get; set; }

    /// <summary>
    /// Trạng thái chính sách mới (Active / Inactive, null nếu không thay đổi).
    /// </summary>
    [MaxLength(20)]
    public string? PricingPolicyStatus { get; set; }
}

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// DTO trả về thông tin chi tiết của một Chính sách giá (PricingPolicy).
/// Bao gồm danh sách các khung giờ (PricingWindow) thuộc chính sách.
/// </summary>
public class PricingPolicyDto
{
    /// <summary>ID chính sách giá.</summary>
    public int Id { get; set; }

    /// <summary>ID loại phương tiện áp dụng.</summary>
    public int VehicleTypeId { get; set; }

    /// <summary>Tên loại phương tiện (denormalized để tiện hiển thị).</summary>
    public string? VehicleTypeName { get; set; }

    /// <summary>Tên chính sách giá.</summary>
    public string PolicyName { get; set; } = null!;

    /// <summary>Ngày bắt đầu hiệu lực.</summary>
    public DateTime EffectiveStart { get; set; }

    /// <summary>Ngày kết thúc hiệu lực (null nếu vô thời hạn).</summary>
    public DateTime? EffectiveEnd { get; set; }

    /// <summary>Trạng thái chính sách (Active / Inactive / Expired).</summary>
    public string PricingPolicyStatus { get; set; } = null!;

    /// <summary>Thời điểm tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Danh sách khung giờ tính giá của chính sách này.</summary>
    public IEnumerable<PricingWindowDto> PricingWindows { get; set; } = new List<PricingWindowDto>();
}

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Chính sách giá (PricingPolicy) lưu cấu hình bảng giá của hệ thống theo loại xe.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class PricingPolicy : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện áp dụng (VehicleType).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Tên chính sách giá (Ví dụ: "Bảng giá vãng lai ô tô").
    /// </summary>
    public string PolicyName { get; set; } = null!;

    /// <summary>
    /// Ngày bắt đầu chính sách có hiệu lực.
    /// </summary>
    public DateTime EffectiveStart { get; set; }

    /// <summary>
    /// Ngày hết hiệu lực của chính sách (cho phép null nếu đang áp dụng vô thời hạn).
    /// </summary>
    public DateTime? EffectiveEnd { get; set; }

    /// <summary>
    /// Trạng thái chính sách giá (Ví dụ: "Active", "Inactive", "Expired").
    /// </summary>
    public string PricingPolicyStatus { get; set; } = "Active";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin loại phương tiện (VehicleType) được áp dụng chính sách này.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Danh sách các khung giờ tính giá (PricingWindow) thuộc chính sách này.
    /// </summary>
    public virtual ICollection<PricingWindow> PricingWindows { get; set; } = new List<PricingWindow>();

    /// <summary>
    /// Danh sách các giao dịch thanh toán (Payment) đã áp dụng bảng giá này.
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
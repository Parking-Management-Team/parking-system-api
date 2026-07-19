namespace PBMS.Domain.Entities;

/// <summary>
/// Cấu hình cho quy tắc tính giá lũy tiến tiếp theo (IncrementPricing).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class IncrementPricingRuleConfig : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới quy tắc tính giá cha (PricingRule).
    /// </summary>
    public int PricingRuleId { get; set; }

    /// <summary>
    /// Kích thước block thời gian phát sinh tiếp theo (Tính bằng phút, ví dụ: 15 phút).
    /// </summary>
    public int IncrementIntervalMinutes { get; set; }

    /// <summary>
    /// Giá tiền của mỗi block phát sinh tiếp theo.
    /// </summary>
    public decimal IncrementPriceAmount { get; set; }

    /// <summary>
    /// Ngưỡng phần trăm tính phí block mới (0-100%).
    /// </summary>
    public int ThresholdPercentage { get; set; }

    /// <summary>
    /// Mã tiền tệ (Mặc định là "VND").
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Quy tắc tính giá chứa cấu hình này.
    /// </summary>
    public virtual PricingRule PricingRule { get; set; } = null!;
}

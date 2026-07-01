namespace PBMS.Domain.Entities;

/// <summary>
/// Cấu hình cho quy tắc tính giá cơ bản (BasePricing).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class BasePricingRuleConfig : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới quy tắc tính giá cha (PricingRule).
    /// </summary>
    public int PricingRuleId { get; set; }

    /// <summary>
    /// Thời lượng cơ bản của block đầu tiên (Tính bằng phút, ví dụ: 60 phút).
    /// </summary>
    public int BaseDurationMinutes { get; set; }

    /// <summary>
    /// Giá của block thời lượng cơ bản đầu tiên.
    /// </summary>
    public decimal BasePriceAmount { get; set; }

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

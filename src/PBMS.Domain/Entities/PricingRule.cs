namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Quy tắc tính giá (PricingRule) liên kết với Chính sách giá.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class PricingRule : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới chính sách giá cha (PricingPolicy).
    /// </summary>
    public int PricingPolicyId { get; set; }

    /// <summary>
    /// Loại quy tắc (Ví dụ: "BasePricing", "IncrementPricing", "DailyCap", "GracePeriod").
    /// </summary>
    public string RuleType { get; set; } = null!;

    /// <summary>
    /// Thứ tự thực thi của quy tắc trong chính sách (từ nhỏ đến lớn).
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của quy tắc.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Chính sách giá chứa quy tắc này.
    /// </summary>
    public virtual PricingPolicy PricingPolicy { get; set; } = null!;

    /// <summary>
    /// Cấu hình quy tắc tính giá cơ bản (nếu RuleType = "BasePricing").
    /// </summary>
    public virtual BasePricingRuleConfig? BasePricingRuleConfig { get; set; }

    /// <summary>
    /// Cấu hình quy tắc tính phí block tiếp theo (nếu RuleType = "IncrementPricing").
    /// </summary>
    public virtual IncrementPricingRuleConfig? IncrementPricingRuleConfig { get; set; }

    /// <summary>
    /// Cấu hình quy tắc giới hạn phí ngày (nếu RuleType = "DailyCap").
    /// </summary>
    public virtual DailyCapRuleConfig? DailyCapRuleConfig { get; set; }

    /// <summary>
    /// Cấu hình quy tắc thời gian ân hạn (nếu RuleType = "GracePeriod").
    /// </summary>
    public virtual GracePeriodRuleConfig? GracePeriodRuleConfig { get; set; }
}

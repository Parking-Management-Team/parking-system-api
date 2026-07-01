namespace PBMS.Domain.Entities;

/// <summary>
/// Cấu hình cho quy tắc thời gian ân hạn (GracePeriod).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class GracePeriodRuleConfig : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới quy tắc tính giá cha (PricingRule).
    /// </summary>
    public int PricingRuleId { get; set; }

    /// <summary>
    /// Thời gian ân hạn sau khi check-in hoặc thanh toán (Tính bằng phút, ví dụ: 15 phút).
    /// </summary>
    public int GracePeriodMinutes { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Quy tắc tính giá chứa cấu hình này.
    /// </summary>
    public virtual PricingRule PricingRule { get; set; } = null!;
}

namespace PBMS.Domain.Entities;

/// <summary>
/// Cấu hình cho quy tắc giới hạn phí ngày (DailyCap).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class DailyCapRuleConfig : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới quy tắc tính giá cha (PricingRule).
    /// </summary>
    public int PricingRuleId { get; set; }

    /// <summary>
    /// Giá trị phí trần tối đa trong 24 giờ của một ngày.
    /// </summary>
    public decimal MaximumDailyAmount { get; set; }

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

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// DTO cấu hình quy tắc tính giá cơ bản (BasePricing).
/// </summary>
public class BasePricingRuleConfigDto
{
    public int Id { get; set; }
    public int BaseDurationMinutes { get; set; }
    public decimal BasePriceAmount { get; set; }
    public string CurrencyCode { get; set; } = "VND";
}

/// <summary>
/// DTO cấu hình quy tắc tính giá lũy tiến tiếp theo (IncrementPricing).
/// </summary>
public class IncrementPricingRuleConfigDto
{
    public int Id { get; set; }
    public int IncrementIntervalMinutes { get; set; }
    public decimal IncrementPriceAmount { get; set; }
    public int ThresholdPercentage { get; set; }
    public string CurrencyCode { get; set; } = "VND";
}

/// <summary>
/// DTO cấu hình quy tắc giới hạn phí ngày (DailyCap).
/// </summary>
public class DailyCapRuleConfigDto
{
    public int Id { get; set; }
    public decimal MaximumDailyAmount { get; set; }
    public string CurrencyCode { get; set; } = "VND";
}

/// <summary>
/// DTO cấu hình quy tắc thời gian ân hạn (GracePeriod).
/// </summary>
public class GracePeriodRuleConfigDto
{
    public int Id { get; set; }
    public int GracePeriodMinutes { get; set; }
}

/// <summary>
/// DTO trả về thông tin chi tiết một Quy tắc tính giá (PricingRule).
/// </summary>
public class PricingRuleDto
{
    public int Id { get; set; }
    public int PricingPolicyId { get; set; }
    public string RuleType { get; set; } = null!;
    public int ExecutionOrder { get; set; }
    public bool IsActive { get; set; }

    public BasePricingRuleConfigDto? BasePricingRuleConfig { get; set; }
    public IncrementPricingRuleConfigDto? IncrementPricingRuleConfig { get; set; }
    public DailyCapRuleConfigDto? DailyCapRuleConfig { get; set; }
    public GracePeriodRuleConfigDto? GracePeriodRuleConfig { get; set; }
}

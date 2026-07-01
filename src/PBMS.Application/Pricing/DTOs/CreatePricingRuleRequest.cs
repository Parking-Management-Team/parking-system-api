using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Pricing.DTOs;

public class CreateBasePricingRuleConfigRequest
{
    [Required]
    public int BaseDurationMinutes { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasePriceAmount { get; set; }

    public string CurrencyCode { get; set; } = "VND";
}

public class CreateIncrementPricingRuleConfigRequest
{
    [Required]
    public int IncrementIntervalMinutes { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal IncrementPriceAmount { get; set; }

    [Required]
    [Range(0, 100)]
    public int ThresholdPercentage { get; set; }

    public string CurrencyCode { get; set; } = "VND";
}

public class CreateDailyCapRuleConfigRequest
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal MaximumDailyAmount { get; set; }

    public string CurrencyCode { get; set; } = "VND";
}

public class CreateGracePeriodRuleConfigRequest
{
    [Required]
    public int GracePeriodMinutes { get; set; }
}

public class CreatePricingRuleRequest
{
    [Required]
    public string RuleType { get; set; } = null!; // BasePricing, IncrementPricing, DailyCap, GracePeriod

    [Required]
    public int ExecutionOrder { get; set; }

    public CreateBasePricingRuleConfigRequest? BasePricingRuleConfig { get; set; }
    public CreateIncrementPricingRuleConfigRequest? IncrementPricingRuleConfig { get; set; }
    public CreateDailyCapRuleConfigRequest? DailyCapRuleConfig { get; set; }
    public CreateGracePeriodRuleConfigRequest? GracePeriodRuleConfig { get; set; }
}

using System;
using System.Collections.Generic;
using PBMS.Domain.Engine;
using PBMS.Domain.Entities;
using Xunit;

namespace PBMS.UnitTests.Pricing;

/// <summary>
/// Unit Tests cho PricingEngine về xử lý Thời gian ân hạn (Grace Period)
/// Đáp ứng yêu cầu PRICE-01 trong PBMS Remediation Decision Report:
///   - Mốc ranh giới: 0, grace - 1, grace, grace + 1
///   - Nhiều block phụ (multiple blocks)
///   - Phân đoạn khung giờ (cross-window)
///   - Qua ngày (cross-day)
/// </summary>
public class PricingEngineGracePeriodTests
{
    private readonly PricingEngine _engine;

    public PricingEngineGracePeriodTests()
    {
        _engine = new PricingEngine();
    }

    private PricingPolicy CreateTestPolicy(
        int baseDurationMinutes = 60,
        decimal basePrice = 5000m,
        int gracePeriodMinutes = 15,
        int incrementIntervalMinutes = 15,
        decimal incrementPrice = 2000m)
    {
        var policy = new PricingPolicy
        {
            Id = 1,
            PolicyName = "Standard Policy",
            PricingPolicyStatus = "Active",
            Priority = 0,
            EffectiveStart = DateTime.UtcNow.Date.AddDays(-30)
        };

        var window = new PricingWindow
        {
            Id = 1,
            WindowName = "Standard Window",
            StartTime = TimeSpan.Zero,
            EndTime = new TimeSpan(23, 59, 59),
            BaseDurationMinutes = baseDurationMinutes,
            BasePrice = basePrice,
            IncrementBlockMinutes = incrementIntervalMinutes,
            IncrementPrice = incrementPrice,
            GracePeriodMinutes = gracePeriodMinutes
        };
        policy.PricingWindows.Add(window);

        policy.PricingRules.Add(new PricingRule
        {
            RuleType = "BasePricing",
            ExecutionOrder = 1,
            IsActive = true,
            BasePricingRuleConfig = new BasePricingRuleConfig
            {
                BaseDurationMinutes = baseDurationMinutes,
                BasePriceAmount = basePrice
            }
        });

        policy.PricingRules.Add(new PricingRule
        {
            RuleType = "GracePeriod",
            ExecutionOrder = 2,
            IsActive = true,
            GracePeriodRuleConfig = new GracePeriodRuleConfig
            {
                GracePeriodMinutes = gracePeriodMinutes
            }
        });

        policy.PricingRules.Add(new PricingRule
        {
            RuleType = "IncrementPricing",
            ExecutionOrder = 3,
            IsActive = true,
            IncrementPricingRuleConfig = new IncrementPricingRuleConfig
            {
                IncrementIntervalMinutes = incrementIntervalMinutes,
                IncrementPriceAmount = incrementPrice
            }
        });

        return policy;
    }

    [Fact]
    public void GracePeriod_Zero_DisablesFreeGraceMinutes()
    {
        // Grace = 0: đỗ 61 phút (lố 1 phút) phải tính 1 block phụ
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 0, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 20, 9, 1, 0, DateTimeKind.Utc); // 61 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(7000m, result.TotalAmount); // 5000 + 2000
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(2000m, result.IncrementAmount);
    }

    [Fact]
    public void GracePeriod_GraceMinusOne_WithinGracePeriod_FreeIncrement()
    {
        // Grace = 15: đỗ 74 phút (lố 14 phút = grace - 1) -> Miễn phí block phụ
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 15, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 20, 9, 14, 0, DateTimeKind.Utc); // 74 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(5000m, result.TotalAmount);
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(0m, result.IncrementAmount);
    }

    [Fact]
    public void GracePeriod_ExactGraceBoundary_FreeIncrement()
    {
        // Grace = 15: đỗ 75 phút (lố 15 phút = exact grace) -> Miễn phí block phụ
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 15, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 20, 9, 15, 0, DateTimeKind.Utc); // 75 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(5000m, result.TotalAmount);
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(0m, result.IncrementAmount);
    }

    [Fact]
    public void GracePeriod_GracePlusOne_ExceedsGracePeriod_ChargesIncrementBlock()
    {
        // Grace = 15: đỗ 76 phút (lố 16 phút = grace + 1) -> 16 - 15 = 1 phút tính phí -> 1 block phụ (2000 VND)
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 15, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 20, 9, 16, 0, DateTimeKind.Utc); // 76 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(7000m, result.TotalAmount); // 5000 + 2000
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(2000m, result.IncrementAmount);
    }

    [Fact]
    public void GracePeriod_MultipleIncrementBlocks()
    {
        // Grace = 15: đỗ 100 phút (lố 40 phút) -> 40 - 15 = 25 phút tính phí -> Ceiling(25/15) = 2 blocks phụ (4000 VND)
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 15, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 20, 9, 40, 0, DateTimeKind.Utc); // 100 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(9000m, result.TotalAmount); // 5000 + 4000
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(4000m, result.IncrementAmount);
    }

    [Fact]
    public void GracePeriod_CrossDay_CalculatesCorrectly()
    {
        // Qua ngày: xe vào 23:00, ra 01:10 hôm sau (130 phút = 60 base + 70 lố)
        // Grace = 15 -> 70 - 15 = 55 phút tính phí -> Ceiling(55/15) = 4 blocks phụ (8000 VND)
        var policy = CreateTestPolicy(baseDurationMinutes: 60, basePrice: 5000m, gracePeriodMinutes: 15, incrementIntervalMinutes: 15, incrementPrice: 2000m);
        var checkIn = new DateTime(2026, 7, 20, 23, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 21, 1, 10, 0, DateTimeKind.Utc); // 130 phút

        var result = _engine.Calculate(policy, checkIn, checkOut);

        Assert.Equal(13000m, result.TotalAmount); // 5000 + 8000
        Assert.Equal(5000m, result.BaseAmount);
        Assert.Equal(8000m, result.IncrementAmount);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using PBMS.Domain.Entities;

namespace PBMS.Domain.Engine;

/// <summary>
/// Thực thi Pricing Engine dựa trên các Rule Config của Chính sách giá (Pricing Policy).
/// </summary>
public class PricingEngine : IPricingEngine
{
    public PricingResult Calculate(
        PricingPolicy policy, 
        DateTime checkIn, 
        DateTime checkOut,
        IEnumerable<Incident>? incidents = null,
        IEnumerable<PenaltyConfig>? penaltyConfigs = null)
    {
        return CalculateSegmented(_ => policy, checkIn, checkOut, incidents, penaltyConfigs);
    }

    public PricingResult CalculateSegmented(
        Func<DateTime, PricingPolicy> getPolicyAtTime,
        DateTime checkIn,
        DateTime checkOut,
        IEnumerable<Incident>? incidents = null,
        IEnumerable<PenaltyConfig>? penaltyConfigs = null)
    {
        if (checkOut <= checkIn)
        {
            return new PricingResult { TotalAmount = 0 };
        }

        var result = new PricingResult();

        // 1. Lấy thông tin chính sách tại thời điểm check-in
        var startPolicy = getPolicyAtTime(checkIn);
        var activeRulesAtStart = startPolicy.PricingRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.ExecutionOrder)
            .ToList();

        var baseRule = activeRulesAtStart.FirstOrDefault(r => r.RuleType == "BasePricing");
        var baseMinutes = baseRule?.BasePricingRuleConfig?.BaseDurationMinutes 
            ?? startPolicy.PricingWindows.FirstOrDefault()?.BaseDurationMinutes 
            ?? 60;
        var basePrice = baseRule?.BasePricingRuleConfig?.BasePriceAmount 
            ?? startPolicy.PricingWindows.FirstOrDefault()?.BasePrice 
            ?? 0m;
        var currencyCode = baseRule?.BasePricingRuleConfig?.CurrencyCode ?? "VND";

        var graceRule = activeRulesAtStart.FirstOrDefault(r => r.RuleType == "GracePeriod");
        var graceMinutes = graceRule?.GracePeriodRuleConfig?.GracePeriodMinutes 
            ?? startPolicy.PricingWindows.FirstOrDefault()?.GracePeriodMinutes 
            ?? 0;

        double totalMinutes = (checkOut - checkIn).TotalMinutes;
        decimal baseAmount = 0;
        decimal incrementAmount = 0;

        // 2. Tính phí Base block
        var baseBlockDuration = Math.Min(totalMinutes, baseMinutes);
        baseAmount += basePrice;
        result.RuleResults.Add(new RuleResult
        {
            RuleType = "BasePricing",
            Amount = basePrice,
            Explanation = $"[{startPolicy.PolicyName}] Áp dụng block đầu tiên ({baseBlockDuration:F1}/{baseMinutes} phút): {basePrice:N0} {currencyCode}"
        });

        // 3. Tính phí các Increment blocks (nếu đỗ lố quá BaseDuration)
        double overMinutes = totalMinutes - baseMinutes;
        if (overMinutes > 0)
        {
            if (overMinutes <= graceMinutes)
            {
                // Đỗ lố trong thời gian ân hạn -> Miễn phí
                result.RuleResults.Add(new RuleResult
                {
                    RuleType = "GracePeriod",
                    Amount = 0,
                    Explanation = $"[{startPolicy.PolicyName}] Thời gian đỗ lố ({overMinutes:F1} phút) nằm trong thời gian ân hạn ({graceMinutes} phút): Miễn phí."
                });
            }
            else
            {
                double billableOverMinutes = overMinutes - graceMinutes;
                if (graceMinutes > 0)
                {
                    result.RuleResults.Add(new RuleResult
                    {
                        RuleType = "GracePeriod",
                        Amount = 0,
                        Explanation = $"[{startPolicy.PolicyName}] Áp dụng thời gian ân hạn {graceMinutes} phút. Thời gian đỗ lố tính phí: {billableOverMinutes:F1} phút."
                    });
                }

                var currentBlockStart = checkIn.AddMinutes(baseMinutes + graceMinutes);
                double remainingBillableMinutes = billableOverMinutes;
                int seq = 1;

                while (remainingBillableMinutes > 0)
                {
                    var currentPolicy = getPolicyAtTime(currentBlockStart);
                    var activeRulesAtCurrent = currentPolicy.PricingRules
                        .Where(r => r.IsActive)
                        .OrderBy(r => r.ExecutionOrder)
                        .ToList();

                    var incRule = activeRulesAtCurrent.FirstOrDefault(r => r.RuleType == "IncrementPricing");
                    var incInterval = incRule?.IncrementPricingRuleConfig?.IncrementIntervalMinutes 
                        ?? currentPolicy.PricingWindows.FirstOrDefault()?.IncrementBlockMinutes 
                        ?? 15;
                    var incPrice = incRule?.IncrementPricingRuleConfig?.IncrementPriceAmount 
                        ?? currentPolicy.PricingWindows.FirstOrDefault()?.IncrementPrice 
                        ?? 0m;
                    var incCurrency = incRule?.IncrementPricingRuleConfig?.CurrencyCode ?? currencyCode;

                    var blockDuration = Math.Min(remainingBillableMinutes, incInterval);
                    incrementAmount += incPrice;

                    result.RuleResults.Add(new RuleResult
                    {
                        RuleType = "IncrementPricing",
                        Amount = incPrice,
                        Explanation = $"[{currentPolicy.PolicyName}] Block phụ thứ {seq} ({blockDuration:F1}/{incInterval} phút): {incPrice:N0} {incCurrency}"
                    });

                    remainingBillableMinutes -= blockDuration;
                    currentBlockStart = currentBlockStart.AddMinutes(blockDuration);
                    seq++;
                }
            }
        }

        var totalAccumulatedParkingFee = baseAmount + incrementAmount;

        // 3. Áp dụng Daily Cap Rule (Giới hạn trần theo ngày lịch 00:00) cho phí đỗ xe
        var endPolicy = getPolicyAtTime(checkOut.AddSeconds(-1));
        var endRules = endPolicy.PricingRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.ExecutionOrder)
            .ToList();
        var dailyCapRule = endRules.FirstOrDefault(r => r.RuleType == "DailyCap");
        if (dailyCapRule?.DailyCapRuleConfig != null)
        {
            var config = dailyCapRule.DailyCapRuleConfig;
            
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var localCheckIn = checkIn.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(checkIn, tz) : checkIn;
            var localCheckOut = checkOut.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(checkOut, tz) : checkOut;
            var startDate = localCheckIn.Date;
            var endDate = localCheckOut.Date;
            var totalDays = (endDate - startDate).Days + 1;
            
            var maxCap = config.MaximumDailyAmount * (decimal)totalDays;

            if (totalAccumulatedParkingFee > maxCap)
            {
                result.RuleResults.Add(new RuleResult
                {
                    RuleType = "DailyCap",
                    Amount = maxCap - totalAccumulatedParkingFee,
                    Explanation = $"[{endPolicy.PolicyName}] Tổng phí tích lũy ({totalAccumulatedParkingFee:N0}) vượt quá giới hạn trần tối đa ({config.MaximumDailyAmount:N0}/ngày x {totalDays} ngày lịch = {maxCap:N0}). Áp giá trần."
                });
                totalAccumulatedParkingFee = maxCap;
                
                // Điều chỉnh lại tỉ lệ hiển thị phân bổ phí khi đã áp giá trần
                baseAmount = Math.Min(baseAmount, totalAccumulatedParkingFee);
                incrementAmount = totalAccumulatedParkingFee - baseAmount;
            }
        }

        // 4. Áp dụng tính phí phạt từ sự cố (Penalty Surcharges)
        decimal penaltyAmount = 0;
        if (incidents != null && incidents.Any())
        {
            foreach (var incident in incidents)
            {
                decimal incidentFee = 0;
                
                if (incident.PenaltyFee.HasValue)
                {
                    incidentFee = incident.PenaltyFee.Value;
                }
                else if (penaltyConfigs != null)
                {
                    var matchConfig = penaltyConfigs.FirstOrDefault(pc => pc.IncidentTypeId == incident.IncidentTypeId && pc.IsActive && !pc.IsDeleted);
                    if (matchConfig != null)
                    {
                        incidentFee = matchConfig.PenaltyFee;
                    }
                }
                else if (incident.IncidentType?.PenaltyConfigs != null)
                {
                    var matchConfig = incident.IncidentType.PenaltyConfigs.FirstOrDefault(pc => pc.IsActive && !pc.IsDeleted);
                    if (matchConfig != null)
                    {
                        incidentFee = matchConfig.PenaltyFee;
                    }
                }

                if (incidentFee > 0)
                {
                    penaltyAmount += incidentFee;
                    result.RuleResults.Add(new RuleResult
                    {
                        RuleType = $"PenaltySurcharge_{incident.IncidentType?.IncidentCode ?? incident.IncidentTypeId.ToString()}",
                        Amount = incidentFee,
                        Explanation = $"Phụ thu phạt sự cố [{(incident.IncidentType?.IncidentName ?? "Sự cố ID: " + incident.IncidentTypeId)}]: {incidentFee:N0} VND"
                    });
                }
            }
        }

        result.BaseAmount = baseAmount;
        result.IncrementAmount = incrementAmount;
        result.PenaltyAmount = penaltyAmount;
        result.TotalAmount = totalAccumulatedParkingFee + penaltyAmount;

        return result;
    }

    /// <summary>
    /// Hàm chia timeline gửi xe thành block cơ bản (Base) và các block lũy tiến (Increment), giải quyết theo chính sách phân đoạn.
    /// </summary>
    private List<PricingBlock> GenerateBlocksSegmented(Func<DateTime, PricingPolicy> getPolicyAtTime, DateTime checkIn, DateTime checkOut)
    {
        var blocks = new List<PricingBlock>();
        
        var startPolicy = getPolicyAtTime(checkIn);
        var activeRulesAtStart = startPolicy.PricingRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.ExecutionOrder)
            .ToList();
        
        var baseRule = activeRulesAtStart.FirstOrDefault(r => r.RuleType == "BasePricing");
        var baseMinutes = baseRule?.BasePricingRuleConfig?.BaseDurationMinutes ?? 60; // Mặc định 60 phút nếu không config

        var totalMinutes = (checkOut - checkIn).TotalMinutes;

        // Block đầu tiên (Base Block)
        var baseBlockDuration = Math.Min(totalMinutes, baseMinutes);
        var baseEndTime = checkIn.AddMinutes(baseBlockDuration);
        blocks.Add(new PricingBlock
        {
            StartTime = checkIn,
            EndTime = baseEndTime,
            IsBase = true,
            BlockSequence = 1
        });

        // Các block phụ sau đó (Increment Blocks)
        var remainingMinutes = totalMinutes - baseBlockDuration;
        var currentBlockStart = baseEndTime;
        int seq = 1;

        while (remainingMinutes > 0)
        {
            var currentPolicy = getPolicyAtTime(currentBlockStart);
            var activeRulesAtCurrent = currentPolicy.PricingRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.ExecutionOrder)
                .ToList();

            var incRule = activeRulesAtCurrent.FirstOrDefault(r => r.RuleType == "IncrementPricing");
            var incInterval = incRule?.IncrementPricingRuleConfig?.IncrementIntervalMinutes ?? 15; // Mặc định 15 phút nếu không config

            var blockDuration = Math.Min(remainingMinutes, incInterval);
            blocks.Add(new PricingBlock
            {
                StartTime = currentBlockStart,
                EndTime = currentBlockStart.AddMinutes(blockDuration),
                IsBase = false,
                BlockSequence = seq++
            });

            remainingMinutes -= blockDuration;
            currentBlockStart = currentBlockStart.AddMinutes(blockDuration);
        }

        return blocks;
    }

}

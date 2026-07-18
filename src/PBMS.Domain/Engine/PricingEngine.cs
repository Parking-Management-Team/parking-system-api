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

        // 1. Chia nhỏ thành các blocks thời gian
        var blocks = GenerateBlocksSegmented(getPolicyAtTime, checkIn, checkOut);
        decimal baseAmount = 0;
        decimal incrementAmount = 0;

        // 2. Thực thi tính phí cho từng block gửi xe
        foreach (var block in blocks)
        {
            var currentPolicy = getPolicyAtTime(block.StartTime);
            var activeRules = currentPolicy.PricingRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.ExecutionOrder)
                .ToList();

            if (block.IsBase)
            {
                var baseRule = activeRules.FirstOrDefault(r => r.RuleType == "BasePricing");
                if (baseRule?.BasePricingRuleConfig != null)
                {
                    var config = baseRule.BasePricingRuleConfig;
                    baseAmount += config.BasePriceAmount;
                    result.RuleResults.Add(new RuleResult
                    {
                        RuleType = "BasePricing",
                        Amount = config.BasePriceAmount,
                        Explanation = $"[{currentPolicy.PolicyName}] Áp dụng block đầu tiên ({block.DurationMinutes:F1}/{config.BaseDurationMinutes} phút): {config.BasePriceAmount:N0} {config.CurrencyCode}"
                    });
                }
            }
            else
            {
                var incRule = activeRules.FirstOrDefault(r => r.RuleType == "IncrementPricing");
                if (incRule?.IncrementPricingRuleConfig != null)
                {
                    var config = incRule.IncrementPricingRuleConfig;
                    var actualMinutes = block.DurationMinutes;
                    var interval = config.IncrementIntervalMinutes;

                    // Tính tỷ lệ xem block lẻ này có vượt ngưỡng ThresholdPercentage hay không
                    var percentage = (actualMinutes / interval) * 100;
                    if (percentage >= config.ThresholdPercentage)
                    {
                        incrementAmount += config.IncrementPriceAmount;
                        result.RuleResults.Add(new RuleResult
                        {
                            RuleType = "IncrementPricing",
                            Amount = config.IncrementPriceAmount,
                            Explanation = $"[{currentPolicy.PolicyName}] Block phụ thứ {block.BlockSequence} ({actualMinutes:F1}/{interval} phút, tỷ lệ {percentage:F0}% >= ngưỡng {config.ThresholdPercentage}%): {config.IncrementPriceAmount:N0} {config.CurrencyCode}"
                        });
                    }
                    else
                    {
                        result.RuleResults.Add(new RuleResult
                        {
                            RuleType = "IncrementPricing",
                            Amount = 0,
                            Explanation = $"[{currentPolicy.PolicyName}] Block phụ thứ {block.BlockSequence} ({actualMinutes:F1}/{interval} phút, tỷ lệ {percentage:F0}% < ngưỡng {config.ThresholdPercentage}%): Miễn phí."
                        });
                    }
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

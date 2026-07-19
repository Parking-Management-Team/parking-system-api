using System;
using System.Collections.Generic;
using PBMS.Domain.Entities;

namespace PBMS.Domain.Engine;

/// <summary>
/// Kết quả tính toán phí của một Rule đơn lẻ.
/// </summary>
public class RuleResult
{
    public string RuleType { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Explanation { get; set; } = null!;
}

/// <summary>
/// Kết quả tổng hợp sau khi chạy qua toàn bộ Engine.
/// </summary>
public class PricingResult
{
    public decimal BaseAmount { get; set; }
    public decimal IncrementAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<RuleResult> RuleResults { get; set; } = new();
}

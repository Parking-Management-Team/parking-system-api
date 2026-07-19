using System;
using PBMS.Domain.Entities;

namespace PBMS.Domain.Engine;

/// <summary>
/// Đại diện cho một block thời gian tính giá sau khi chia nhỏ.
/// </summary>
public class PricingBlock
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DurationMinutes => (EndTime - StartTime).TotalMinutes;
    public bool IsBase { get; set; }
    public int BlockSequence { get; set; }
}

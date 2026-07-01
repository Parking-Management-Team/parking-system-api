using System;
using System.Collections.Generic;
using PBMS.Domain.Entities;

namespace PBMS.Domain.Engine;

/// <summary>
/// Giao diện Engine tính toán giá vé.
/// </summary>
public interface IPricingEngine
{
    /// <summary>
    /// Tính toán chi phí gửi xe dựa trên chính sách giá, khoảng thời gian và các sự cố phạt đi kèm.
    /// </summary>
    PricingResult Calculate(
        PricingPolicy policy, 
        DateTime checkIn, 
        DateTime checkOut,
        IEnumerable<Incident>? incidents = null,
        IEnumerable<PenaltyConfig>? penaltyConfigs = null);
}

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
    /// Tính toán chi phí gửi xe dựa trên chính sách giá và khoảng thời gian gửi xe.
    /// </summary>
    PricingResult Calculate(PricingPolicy policy, DateTime checkIn, DateTime checkOut);
}

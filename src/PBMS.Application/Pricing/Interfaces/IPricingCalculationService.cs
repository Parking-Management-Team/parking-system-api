using System;
using System.Threading.Tasks;
using PBMS.Domain.Engine;

namespace PBMS.Application.Pricing.Interfaces;

/// <summary>
/// Giao diện dịch vụ tính toán phí gửi xe bằng Rule-based Pricing Engine mới.
/// Thay thế IFeeCalculationService cũ.
/// </summary>
public interface IPricingCalculationService
{
    /// <summary>
    /// Tính toán tổng phí gửi xe dựa trên cấu hình Pricing Policy và các Rules đã cài đặt.
    /// </summary>
    /// <param name="vehicleTypeId">ID loại phương tiện</param>
    /// <param name="checkIn">Thời điểm xe vào bãi</param>
    /// <param name="checkOut">Thời điểm xe ra bãi</param>
    /// <returns>PricingResult chứa tổng tiền và chi tiết chạy của từng rule</returns>
    Task<PricingResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut);

    /// <summary>
    /// Tính toán phí gửi xe và ghi log audit vào cơ sở dữ liệu.
    /// </summary>
    Task<PricingResult> CalculateFeeAndLogAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? bookingId = null, int? parkingSessionId = null);
}

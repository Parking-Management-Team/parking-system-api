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
    /// Tính toán xem trước phí gửi xe (Preview only). KHÔNG ghi log vào CSDL.
    /// </summary>
    Task<PricingResult> CalculatePreviewAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? parkingSessionId = null);

    /// <summary>
    /// Tính toán tổng phí gửi xe dựa trên cấu hình Pricing Policy (Preview only). Alias cho CalculatePreviewAsync.
    /// </summary>
    Task<PricingResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? parkingSessionId = null);

    /// <summary>
    /// Tính toán và ghi duy nhất 1 bản ghi PricingCalculationLog đối soát giao dịch tài chính chính thức (Committed).
    /// Hỗ trợ kiểm tra idempotency để tránh ghi trùng log.
    /// </summary>
    Task<PricingResult> CalculateCommittedFeeAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut,
        string calculationPurpose,
        int? bookingId = null,
        int? parkingSessionId = null,
        int? paymentId = null,
        string? idempotencyKey = null);

    /// <summary>
    /// Tính toán phí gửi xe và ghi log audit (Backward compatibility alias cho CalculateCommittedFeeAsync).
    /// </summary>
    Task<PricingResult> CalculateFeeAndLogAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut,
        int? bookingId = null,
        int? parkingSessionId = null,
        string calculationPurpose = "CHECKOUT_FINAL",
        int? paymentId = null,
        string? idempotencyKey = null);
}

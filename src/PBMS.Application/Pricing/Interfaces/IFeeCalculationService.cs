using PBMS.Application.Pricing.DTOs;
using PBMS.Domain.Entities;

namespace PBMS.Application.Pricing.Interfaces;

/// <summary>
/// Giao diện dịch vụ tính toán phí gửi xe (Fee Calculation Service).
///
/// Triển khai logic tính phí theo Business Rules BR-FEE-001 đến BR-FEE-012:
///   - Phân tách thời gian đỗ theo các khung giờ (BR-FEE-003, BR-FEE-006).
///   - Áp dụng BasePrice cho block đầu tiên (BR-FEE-004).
///   - Tính block lũy tiến phát sinh sau BaseDuration (BR-FEE-005).
///   - Giới hạn phí theo WindowCap trong từng khung giờ (BR-FEE-007).
///   - Xử lý thời gian ân hạn GracePeriod (BR-FEE-011, BR-FEE-012).
///   - Hệ thống vận hành 24/7, không reset khi qua ngày (BR-FEE-008).
/// </summary>
public interface IFeeCalculationService
{
    /// <summary>
    /// Tính toán tổng phí gửi xe cho một khoảng thời gian cụ thể.
    ///
    /// Thuật toán:
    ///   1. Xác định PricingPolicy đang Active cho loại xe tại thời điểm checkIn.
    ///   2. Phân tách [checkIn, checkOut] thành các đoạn thời gian theo PricingWindow.
    ///   3. Với mỗi đoạn: tính BaseCharge + IncrementCharge, áp WindowCap, áp GracePeriod.
    ///   4. Cộng tổng phí tất cả các đoạn.
    ///
    /// </summary>
    /// <param name="vehicleTypeId">Loại xe để tra cứu PricingPolicy phù hợp.</param>
    /// <param name="checkIn">Thời điểm xe vào bãi.</param>
    /// <param name="checkOut">Thời điểm xe ra bãi (tính phí).</param>
    /// <returns>FeeCalculationResult chứa tổng phí và chi tiết từng đoạn.</returns>
    Task<FeeCalculationResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut);

    /// <summary>
    /// Tính toán phí trực tiếp từ danh sách PricingWindows đã được truyền vào.
    /// Dùng cho unit testing và các luồng không cần truy vấn DB.
    ///
    /// </summary>
    /// <param name="windows">Danh sách khung giờ của chính sách giá đang áp dụng.</param>
    /// <param name="checkIn">Thời điểm xe vào bãi.</param>
    /// <param name="checkOut">Thời điểm xe ra bãi.</param>
    /// <returns>FeeCalculationResult chứa tổng phí và chi tiết từng đoạn.</returns>
    FeeCalculationResult CalculateFeeFromWindows(IEnumerable<PricingWindow> windows, DateTime checkIn, DateTime checkOut);
}

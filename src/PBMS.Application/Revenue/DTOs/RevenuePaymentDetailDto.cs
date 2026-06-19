using System;
namespace PBMS.Application.Revenue.DTOs;

/// <summary>
/// DTO chứa chi tiết giao dịch thanh toán phục vụ đối soát doanh thu 
/// </summary>

public class RevenuePaymentDetailDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime? PaymentTime { get; set; }

    /// <summary>
    /// Nguồn thanh toán: "Session", "Booking", hoặc "MonthlySubscription".
    /// </summary>
    public string SourceType { get; set; } = null!;

    /// <summary>
    /// Biển số xe liên quan (nếu có).
    /// </summary>
    public string? LicensePlate { get; set; }


}
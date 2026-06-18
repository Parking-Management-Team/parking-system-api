using System;

namespace PBMS.Application.Payment.DTOs;

/// <summary>
/// Đối tượng dữ liệu phản hồi thông tin giao dịch thanh toán.
/// </summary>
public class PaymentResponseDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? BookingId { get; set; }
    public int? MonthlySubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public DateTime? PaymentTime { get; set; }
    public long? OrderCode { get; set; }

    /// <summary>
    /// Đường dẫn trang thanh toán PayOS (chỉ xuất hiện đối với hình thức ONLINE_BANKING).
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// Mã QR thanh toán VietQR chuyển khoản (chỉ xuất hiện đối với hình thức ONLINE_BANKING).
    /// </summary>
    public string? QrCodeUrl { get; set; }
}

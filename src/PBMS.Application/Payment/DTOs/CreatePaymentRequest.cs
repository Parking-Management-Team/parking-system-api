namespace PBMS.Application.Payment.DTOs;

/// <summary>
/// Đối tượng yêu cầu tạo mới giao dịch thanh toán.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    /// ID lượt gửi xe (nếu thanh toán cho lượt gửi xe đỗ xe vãng lai/trả sau).
    /// </summary>
    public int? SessionId { get; set; }

    /// <summary>
    /// ID đặt chỗ trước (nếu thanh toán tiền đặt cọc booking).
    /// </summary>
    public int? BookingId { get; set; }

    /// <summary>
    /// ID vé tháng (nếu thanh toán đăng ký/gia hạn vé tháng).
    /// </summary>
    public int? MonthlySubscriptionId { get; set; }

    /// <summary>
    /// Phương thức thanh toán: "CASH" (Tiền mặt) hoặc "ONLINE_BANKING" (Chuyển khoản qua cổng PayOS).
    /// </summary>
    public string PaymentMethod { get; set; } = null!;
}

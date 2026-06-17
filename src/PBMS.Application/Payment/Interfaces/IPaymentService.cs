using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Payment.DTOs;

namespace PBMS.Application.Payment.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ Thanh Toán (Payment Service).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Khởi tạo một giao dịch thanh toán mới (Tiền mặt làm tròn hoặc Online qua PayOS).
    /// </summary>
    Task<BaseResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequest request);

    /// <summary>
    /// Xử lý dữ liệu Webhook từ cổng thanh toán PayOS gọi về.
    /// </summary>
    Task<BaseResponse<string>> ProcessWebhookAsync(PayOSWebhookRequest webhookRequest);
}

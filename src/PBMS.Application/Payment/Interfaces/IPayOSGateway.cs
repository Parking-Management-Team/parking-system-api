using System.Threading.Tasks;

namespace PBMS.Application.Payment.Interfaces;

/// <summary>
/// Giao diện kết nối cổng thanh toán PayOS.
/// </summary>
public interface IPayOSGateway
{
    /// <summary>
    /// Tạo link thanh toán PayOS và mã QR VietQR tương ứng.
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng kiểu số 64-bit.</param>
    /// <param name="amount">Số tiền thanh toán chính xác.</param>
    /// <param name="description">Nội dung chuyển khoản.</param>
    /// <returns>Trả về checkoutUrl và qrCodeUrl.</returns>
    Task<(string CheckoutUrl, string QrCode)> CreatePaymentLinkAsync(long orderCode, decimal amount, string description);

    /// <summary>
    /// Xác thực chữ ký dữ liệu phản hồi (Webhook) gửi từ PayOS.
    /// </summary>
    /// <param name="dataString">Chuỗi JSON hoặc chuỗi data dạng key=value đã nối từ data.</param>
    /// <param name="signature">Chữ ký signature do PayOS gửi sang để đối chiếu.</param>
    /// <returns>True nếu chữ ký hợp lệ, ngược lại False.</returns>
    bool VerifyWebhookSignature(string dataString, string signature);
}

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
    /// Xử lý dữ liệu phản hồi (IPN) tự động từ cổng thanh toán VNPay gọi về.
    /// </summary>
    Task<BaseResponse<string>> ProcessVNPayIPNAsync(System.Collections.Generic.SortedDictionary<string, string> vnpayData);

    /// <summary>
    /// Lấy danh sách giao dịch thanh toán phân trang (dành cho Admin/Staff).
    /// </summary>
    Task<PagedResult<PaymentResponseDto>> GetPaymentsPagedAsync(
        int pageIndex,
        int pageSize,
        DateTime? fromDate,
        DateTime? toDate,
        string? method);

    /// <summary>
    /// Lấy lịch sử giao dịch thanh toán của một lượt gửi xe (Session).
    /// </summary>
    Task<System.Collections.Generic.IEnumerable<PaymentResponseDto>> GetPaymentsBySessionIdAsync(int sessionId);

    /// <summary>
    /// Lấy lịch sử giao dịch thanh toán của một tài khoản (Account).
    /// </summary>
    Task<System.Collections.Generic.IEnumerable<PaymentResponseDto>> GetPaymentsByAccountIdAsync(int accountId);

    /// <summary>
    /// Xử lý hoàn tiền cho giao dịch đang ở trạng thái REFUND_PENDING.
    /// </summary>
    Task<BaseResponse<PaymentResponseDto>> ProcessRefundAsync(int paymentId);
}

using System.Collections.Generic;

namespace PBMS.Application.Payment.Interfaces;

/// <summary>
/// Giao diện kết nối cổng thanh toán VNPay.
/// </summary>
public interface IVNPayGateway
{
    /// <summary>
    /// Tạo link thanh toán VNPay.
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng kiểu số 64-bit.</param>
    /// <param name="amount">Số tiền thanh toán.</param>
    /// <param name="description">Nội dung thanh toán.</param>
    /// <param name="ipAddress">Địa chỉ IP của client tạo thanh toán.</param>
    /// <returns>Trả về URL redirect sang cổng VNPay.</returns>
    string CreatePaymentUrl(long orderCode, decimal amount, string description, string ipAddress);

    /// <summary>
    /// Xác thực chữ ký phản hồi của VNPay (IPN/Return).
    /// </summary>
    /// <param name="fields">Tập hợp các tham số nhận về từ VNPay.</param>
    /// <param name="secureHash">Chữ ký secureHash nhận về từ VNPay.</param>
    /// <returns>True nếu hợp lệ, ngược lại False.</returns>
    bool VerifySignature(SortedDictionary<string, string> fields, string secureHash);
}

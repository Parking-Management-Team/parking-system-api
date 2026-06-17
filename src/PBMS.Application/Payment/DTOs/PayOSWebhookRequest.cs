namespace PBMS.Application.Payment.DTOs;

/// <summary>
/// Cấu trúc dữ liệu Webhook gửi đến từ PayOS.
/// </summary>
public class PayOSWebhookRequest
{
    public string Code { get; set; } = null!;
    public string Desc { get; set; } = null!;

    /// <summary>
    /// Chữ ký bảo mật HMAC-SHA256 của PayOS để xác thực dữ liệu.
    /// </summary>
    public string Signature { get; set; } = null!;

    /// <summary>
    /// Phần dữ liệu giao dịch chi tiết.
    /// </summary>
    public PayOSWebhookData Data { get; set; } = null!;
}

/// <summary>
/// Dữ liệu chi tiết giao dịch từ PayOS.
/// </summary>
public class PayOSWebhookData
{
    /// <summary>
    /// Mã đơn hàng 64-bit chúng ta đã gửi đi.
    /// </summary>
    public long OrderCode { get; set; }

    /// <summary>
    /// Số tiền khách đã thanh toán.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Nội dung giao dịch.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Trạng thái của đơn hàng từ cổng thanh toán (ví dụ: "PAID").
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Mã đối soát giao dịch ngân hàng ngân hàng.
    /// </summary>
    public string Reference { get; set; } = null!;

    /// <summary>
    /// Số tài khoản ngân hàng nhận tiền.
    /// </summary>
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// Thời gian xảy ra giao dịch.
    /// </summary>
    public string TransactionDateTime { get; set; } = null!;
}

using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.Payment.DTOs;
using PBMS.Application.Payment.Interfaces;
using System;
using System.Threading.Tasks;

namespace PBMS.API.Controllers
{
    /// <summary>
    /// API Controller quản lý và xử lý luồng thanh toán (Tiền mặt và PayOS).
    /// Route chính: /api/payments
    /// </summary>
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        // Constructor thực hiện inject dịch vụ IPaymentService đã được đăng ký DI trước đó
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        /// <summary>
        /// API tạo một yêu cầu thanh toán mới (CASH hoặc ONLINE_BANKING).
        /// Route: POST /api/payments
        /// </summary>
        /// <param name="request">DTO chứa phương thức thanh toán và nguồn thanh toán (Lượt đỗ/Booking/Vé tháng).</param>
        /// <returns>Thông tin giao dịch được tạo kèm link QR nếu chọn chuyển khoản ngân hàng.</returns>
        [HttpPost]
        public async Task<ActionResult<BaseResponse<PaymentResponseDto>>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var response = await _paymentService.CreatePaymentAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// API Endpoint nhận Webhook callback tự động từ cổng PayOS khi chuyển khoản thành công.
        /// Route: POST /api/payments/callback
        /// </summary>
        /// <param name="webhookRequest">Mẫu dữ liệu chuẩn hóa của webhook kèm chữ ký mã hóa từ PayOS.</param>
        /// <returns>Xác nhận xử lý thành công hoặc thông báo lỗi chữ ký giả mạo.</returns>
        [HttpPost("callback")]
        public async Task<ActionResult<BaseResponse<string>>> PayOSCallback([FromBody] PayOSWebhookRequest webhookRequest)
        {
            var response = await _paymentService.ProcessWebhookAsync(webhookRequest);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}

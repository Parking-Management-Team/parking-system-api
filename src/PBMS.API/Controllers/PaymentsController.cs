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

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        /// <summary>
        /// API tạo một yêu cầu thanh toán mới (CASH hoặc ONLINE_BANKING).
        /// Route: POST /api/payments
        /// </summary>
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
        /// API Endpoint nhận IPN callback tự động từ VNPay khi chuyển khoản thành công.
        /// Route: GET /api/payments/callback
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> VNPayIPNCallback()
        {
            var collections = Request.Query;
            var vnpayData = new SortedDictionary<string, string>(StringComparer.Ordinal);
            
            foreach (var key in collections.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpayData.Add(key, collections[key]!);
                }
            }

            var response = await _paymentService.ProcessVNPayIPNAsync(vnpayData);

            if (response.Success)
            {
                // Trả về định dạng JSON bắt buộc của VNPay để hoàn tất ghi nhận
                return Ok(new { RspCode = response.Data ?? "00", Message = response.Message ?? "Confirm success" });
            }

            // Map mã lỗi theo đặc tả VNPay IPN
            string rspCode = response.ErrorCode switch
            {
                "INVALID_SIGNATURE" => "97",
                "PAYMENT_NOT_FOUND" => "01",
                "ORDER_ALREADY_CONFIRMED" => "02",
                _ => "99"
            };

            return Ok(new { RspCode = rspCode, Message = response.Message ?? "Error occurred" });
        }

        /// <summary>
        /// GET /api/payments
        /// Lấy danh sách tất cả giao dịch thanh toán phân trang (dành cho Admin/Staff).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<BaseResponse<PagedResult<PaymentResponseDto>>>> GetPaymentsPaged(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? method,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _paymentService.GetPaymentsPagedAsync(pageIndex, pageSize, fromDate, toDate, method);
            return Ok(BaseResponse<PagedResult<PaymentResponseDto>>.Ok(result));
        }

        /// <summary>
        /// GET /api/payments/by-session/{sessionId}
        /// Lấy danh sách giao dịch thanh toán theo sessionId.
        /// </summary>
        [HttpGet("by-session/{sessionId:int}")]
        public async Task<ActionResult<BaseResponse<System.Collections.Generic.IEnumerable<PaymentResponseDto>>>> GetPaymentsBySession(int sessionId)
        {
            var result = await _paymentService.GetPaymentsBySessionIdAsync(sessionId);
            return Ok(BaseResponse<System.Collections.Generic.IEnumerable<PaymentResponseDto>>.Ok(result));
        }

        /// <summary>
        /// GET /api/payments/by-account/{accountId}
        /// Lấy danh sách giao dịch thanh toán theo accountId.
        /// </summary>
        [HttpGet("by-account/{accountId:int}")]
        public async Task<ActionResult<BaseResponse<System.Collections.Generic.IEnumerable<PaymentResponseDto>>>> GetPaymentsByAccount(int accountId)
        {
            var result = await _paymentService.GetPaymentsByAccountIdAsync(accountId);
            return Ok(BaseResponse<System.Collections.Generic.IEnumerable<PaymentResponseDto>>.Ok(result));
        }
    }
}

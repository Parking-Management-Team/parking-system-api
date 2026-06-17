using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.Payment.DTOs;
using PBMS.Application.Payment.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.Payment.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ Thanh Toán (Payment Service).
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IRepository<PBMS.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<MonthlySubscription> _subscriptionRepository;
    private readonly IRepository<PBMS.Domain.Entities.Vehicle> _vehicleRepository;
    private readonly IVNPayGateway _vnpayGateway;
    private readonly IParkingSessionService _sessionService;
    private readonly IFeeCalculationService _feeCalculationService;
    private readonly IConfiguration _configuration;

    public PaymentService(
        IRepository<PBMS.Domain.Entities.Payment> paymentRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IRepository<Booking> bookingRepository,
        IRepository<MonthlySubscription> subscriptionRepository,
        IRepository<PBMS.Domain.Entities.Vehicle> vehicleRepository,
        IVNPayGateway vnpayGateway,
        IParkingSessionService sessionService,
        IFeeCalculationService feeCalculationService,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _subscriptionRepository = subscriptionRepository;
        _vehicleRepository = vehicleRepository;
        _vnpayGateway = vnpayGateway;
        _sessionService = sessionService;
        _feeCalculationService = feeCalculationService;
        _configuration = configuration;
    }

    /// <summary>
    /// Tạo thanh toán mới.
    /// </summary>
    public async Task<BaseResponse<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        // 1. Kiểm tra ràng buộc duy nhất nguồn thanh toán
        int sourceCount = (request.SessionId.HasValue ? 1 : 0) +
                          (request.BookingId.HasValue ? 1 : 0) +
                          (request.MonthlySubscriptionId.HasValue ? 1 : 0);

        if (sourceCount != 1)
        {
            return BaseResponse<PaymentResponseDto>.Fail("INVALID_PAYMENT_SOURCE", "The payment transaction must be linked to exactly one source: SessionId, BookingId, or MonthlySubscriptionId.");
        }

        decimal originalAmount = 0;
        string description = "Transaction payment";

        // 2. Xác định số tiền gốc cần thanh toán theo từng nguồn nghiệp vụ
        if (request.SessionId.HasValue)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId.Value);
            if (session == null)
                return BaseResponse<PaymentResponseDto>.Fail("SESSION_NOT_FOUND", "Parking session not found.");

            if (session.SessionStatus.ToUpper() == "COMPLETED")
                return BaseResponse<PaymentResponseDto>.Fail("SESSION_ALREADY_COMPLETED", "This parking session has already been completed and paid.");

            // Lấy thông tin xe để biết loại xe (VehicleType) phục vụ tính giá
            var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
            if (vehicle == null)
                return BaseResponse<PaymentResponseDto>.Fail("VEHICLE_NOT_FOUND", "Vehicle information not found.");

            // Tính toán tiền đỗ xe thực tế dựa vào thời điểm check-in & check-out
            var checkOutTime = session.CheckOutTime ?? DateTime.UtcNow;
            var feeResult = await _feeCalculationService.CalculateFeeAsync(vehicle.VehicleTypeId, session.CheckInTime, checkOutTime);

            decimal finalFee = feeResult.TotalFee;
            // Nếu lượt gửi xe có liên kết với đặt chỗ, thực hiện khấu trừ tiền đặt cọc vào tổng tiền thanh toán
            if (session.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdAsync(session.BookingId.Value);
                if (booking != null)
                {
                    finalFee = Math.Max(0, finalFee - booking.DepositAmount);
                }
            }

            originalAmount = finalFee;
            description = $"Parking fee payment for session {session.Id}";

        }
        else if (request.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId.Value);
            if (booking == null)
                return BaseResponse<PaymentResponseDto>.Fail("BOOKING_NOT_FOUND", "Booking information not found.");

            originalAmount = booking.DepositAmount;
            description = $"Deposit payment for booking {booking.Id}";
        }
        else if (request.MonthlySubscriptionId.HasValue)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(request.MonthlySubscriptionId.Value);
            if (subscription == null)
                return BaseResponse<PaymentResponseDto>.Fail("SUBSCRIPTION_NOT_FOUND", "Monthly subscription information not found.");

            originalAmount = subscription.MonthlyPrice;
            description = $"Monthly subscription payment for subscription {subscription.Id}";
        }

        // 3. Xử lý logic theo Phương thức thanh toán
        var method = request.PaymentMethod.ToUpperInvariant();

        if (method == "CASH")
        {
            // --- Luồng Thanh toán Tiền mặt (Áp dụng Cash Rounding) ---
            var roundingUnit = decimal.Parse(_configuration["PaymentSettings:CashRoundingUnit"] ?? "500");
            var roundingThreshold = decimal.Parse(_configuration["PaymentSettings:RoundingThreshold"] ?? "250");

            var roundingPolicy = new CashRoundingPolicy();
            decimal roundedAmount = roundingPolicy.Round(originalAmount, roundingUnit, roundingThreshold);

            // Tạo thực thể Payment đã thanh toán thành công (PAID)
            var payment = new PBMS.Domain.Entities.Payment
            {
                SessionId = request.SessionId,
                BookingId = request.BookingId,
                MonthlySubscriptionId = request.MonthlySubscriptionId,
                Amount = roundedAmount,
                PaymentMethod = "CASH",
                PaymentStatus = "PAID",
                PaymentTime = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // Hoàn tất nghiệp vụ logic sau khi thanh toán thành công
            await CompleteBusinessFlowAsync(payment);

            return BaseResponse<PaymentResponseDto>.Ok(MapToDto(payment), "Cash payment successful (Cash rounding applied).");
        }
        else if (method == "ONLINE_BANKING")
        {
            // --- Luồng Thanh toán Online qua VNPay ---
            // Sinh mã orderCode số nguyên 64-bit ngẫu nhiên duy nhất
            long orderCode = new Random().NextInt64(10000000, 99999999);

            var payment = new PBMS.Domain.Entities.Payment
            {
                SessionId = request.SessionId,
                BookingId = request.BookingId,
                MonthlySubscriptionId = request.MonthlySubscriptionId,
                Amount = originalAmount,
                PaymentMethod = "ONLINE_BANKING",
                PaymentStatus = "PENDING",
                OrderCode = orderCode
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            try
            {
                // Gọi tới VNPay Gateway để tạo URL thanh toán
                // Giả lập địa chỉ IP khách hàng là 127.0.0.1 nếu chạy test local
                string paymentUrl = _vnpayGateway.CreatePaymentUrl(orderCode, originalAmount, description, "127.0.0.1");

                var responseDto = MapToDto(payment);
                responseDto.PaymentUrl = paymentUrl;
                responseDto.QrCodeUrl = ""; // VNPay đã tích hợp sẵn QR trong trang thanh toán

                return BaseResponse<PaymentResponseDto>.Ok(responseDto, "VNPay payment link created successfully.");
            }
            catch (Exception ex)
            {
                // Nếu lỗi tạo link, đưa trạng thái giao dịch về FAILED
                payment.PaymentStatus = "FAILED";
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangesAsync();

                return BaseResponse<PaymentResponseDto>.Fail("GATEWAY_ERROR", "VNPay payment gateway error: " + ex.Message);
            }
        }

        return BaseResponse<PaymentResponseDto>.Fail("INVALID_PAYMENT_METHOD", "Unsupported payment method (only CASH or ONLINE_BANKING is accepted).");
    }

    /// <summary>
    /// Xử lý IPN callback từ VNPay phản hồi chuyển khoản.
    /// </summary>
    public async Task<BaseResponse<string>> ProcessVNPayIPNAsync(System.Collections.Generic.SortedDictionary<string, string> vnpayData)
    {
        if (!vnpayData.TryGetValue("vnp_SecureHash", out string? secureHash) || string.IsNullOrEmpty(secureHash))
        {
            return BaseResponse<string>.Fail("MISSING_SIGNATURE", "Secure hash vnp_SecureHash not found.");
        }

        // 1. Xác thực tính toàn vẹn của chữ ký số từ VNPay
        bool isSignatureValid = _vnpayGateway.VerifySignature(vnpayData, secureHash);
        if (!isSignatureValid)
        {
            return BaseResponse<string>.Fail("INVALID_SIGNATURE", "VNPay IPN signature verification failed.");
        }

        // 2. Đọc mã đơn hàng vnp_TxnRef
        if (!vnpayData.TryGetValue("vnp_TxnRef", out string? txnRef) || !long.TryParse(txnRef, out long orderCode))
        {
            return BaseResponse<string>.Fail("INVALID_TXN_REF", "Transaction code vnp_TxnRef is invalid.");
        }

        // Tìm giao dịch thanh toán trong hệ thống qua orderCode
        var payment = (await _paymentRepository.FindAsync(p => p.OrderCode == orderCode)).FirstOrDefault();
        if (payment == null)
        {
            return BaseResponse<string>.Fail("PAYMENT_NOT_FOUND", $"Transaction not found with order code {orderCode}.");
        }

        if (payment.PaymentStatus == "PENDING")
        {
            vnpayData.TryGetValue("vnp_ResponseCode", out string? responseCode);
            vnpayData.TryGetValue("vnp_TransactionStatus", out string? transactionStatus);

            // "00" biểu thị giao dịch thanh toán thành công trong VNPay
            if (responseCode == "00" && transactionStatus == "00")
            {
                payment.PaymentStatus = "PAID";
                payment.PaymentTime = DateTime.UtcNow;
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangesAsync();

                // Hoàn tất luồng nghiệp vụ tương ứng
                await CompleteBusinessFlowAsync(payment);

                return BaseResponse<string>.Ok("00", "Confirm success");
            }
            else
            {
                payment.PaymentStatus = "FAILED";
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangesAsync();

                return BaseResponse<string>.Ok("00", $"Payment failed from VNPay, response code: {responseCode}");
            }
        }

        return BaseResponse<string>.Ok("02", "Order already confirmed");
    }

    /// <summary>
    /// Hàm phụ trợ hoàn tất luồng nghiệp vụ hệ thống dựa vào nguồn thanh toán sau khi giao dịch thành công.
    /// </summary>
    private async Task CompleteBusinessFlowAsync(PBMS.Domain.Entities.Payment payment)
    {
        if (payment.SessionId.HasValue)
        {
            // Thanh toán đỗ xe -> Hoàn tất lượt đỗ (Gọi IParkingSessionService để đổi trạng thái thành COMPLETED)
            await _sessionService.CompleteAsync(payment.SessionId.Value);
        }
        else if (payment.BookingId.HasValue)
        {
            // Thanh toán đặt cọc -> Xác nhận đặt cọc thành công
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId.Value);
            if (booking != null)
            {
                booking.BookingStatus = "Confirmed";
                booking.ConfirmedAt = DateTime.UtcNow;
                _bookingRepository.Update(booking);
                await _bookingRepository.SaveChangesAsync();
            }
        }
        else if (payment.MonthlySubscriptionId.HasValue)
        {
            // Thanh toán vé tháng -> Kích hoạt vé tháng và tính thời điểm hết hạn
            var subscription = await _subscriptionRepository.GetByIdAsync(payment.MonthlySubscriptionId.Value);
            if (subscription != null)
            {
                subscription.MonthlySubscriptionStatus = "ACTIVE";
                subscription.ActivatedAt = DateTime.UtcNow;
                subscription.ExpiredAt = DateTime.UtcNow.AddMonths(1); // Vé tháng có giá trị 1 tháng
                _subscriptionRepository.Update(subscription);
                await _subscriptionRepository.SaveChangesAsync();
            }
        }
    }

    private static PaymentResponseDto MapToDto(PBMS.Domain.Entities.Payment payment) => new()
    {
        Id = payment.Id,
        SessionId = payment.SessionId,
        BookingId = payment.BookingId,
        MonthlySubscriptionId = payment.MonthlySubscriptionId,
        Amount = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        PaymentStatus = payment.PaymentStatus,
        PaymentTime = payment.PaymentTime,
        OrderCode = payment.OrderCode
    };
}

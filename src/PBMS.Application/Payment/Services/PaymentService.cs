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
    private readonly IPayOSGateway _payOSGateway;
    private readonly IParkingSessionService _sessionService;
    private readonly IFeeCalculationService _feeCalculationService;
    private readonly IConfiguration _configuration;

    public PaymentService(
        IRepository<PBMS.Domain.Entities.Payment> paymentRepository,
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        IRepository<Booking> bookingRepository,
        IRepository<MonthlySubscription> subscriptionRepository,
        IRepository<PBMS.Domain.Entities.Vehicle> vehicleRepository,
        IPayOSGateway payOSGateway,
        IParkingSessionService sessionService,
        IFeeCalculationService feeCalculationService,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _subscriptionRepository = subscriptionRepository;
        _vehicleRepository = vehicleRepository;
        _payOSGateway = payOSGateway;
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
            return BaseResponse<PaymentResponseDto>.Fail("INVALID_PAYMENT_SOURCE", "Giao dịch thanh toán phải liên kết chính xác tới duy nhất 1 nguồn: SessionId, BookingId hoặc MonthlySubscriptionId.");
        }

        decimal originalAmount = 0;
        string description = "Thanh toan giao dich";

        // 2. Xác định số tiền gốc cần thanh toán theo từng nguồn nghiệp vụ
        if (request.SessionId.HasValue)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId.Value);
            if (session == null)
                return BaseResponse<PaymentResponseDto>.Fail("SESSION_NOT_FOUND", "Không tìm thấy lượt đỗ xe tương ứng.");

            if (session.SessionStatus.ToUpper() == "COMPLETED")
                return BaseResponse<PaymentResponseDto>.Fail("SESSION_ALREADY_COMPLETED", "Lượt đỗ xe này đã hoàn tất thanh toán và đóng từ trước.");

            // Lấy thông tin xe để biết loại xe (VehicleType) phục vụ tính giá
            var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);
            if (vehicle == null)
                return BaseResponse<PaymentResponseDto>.Fail("VEHICLE_NOT_FOUND", "Không tìm thấy thông tin phương tiện gửi xe.");

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
            description = $"Thanh toan phi do xe luot {session.Id}";

        }
        else if (request.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId.Value);
            if (booking == null)
                return BaseResponse<PaymentResponseDto>.Fail("BOOKING_NOT_FOUND", "Không tìm thấy thông tin đặt chỗ.");

            originalAmount = booking.DepositAmount;
            description = $"Thanh toan tien coc dat cho {booking.Id}";
        }
        else if (request.MonthlySubscriptionId.HasValue)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(request.MonthlySubscriptionId.Value);
            if (subscription == null)
                return BaseResponse<PaymentResponseDto>.Fail("SUBSCRIPTION_NOT_FOUND", "Không tìm thấy thông tin đăng ký vé tháng.");

            originalAmount = subscription.MonthlyPrice;
            description = $"Thanh toan phi dang ky ve thang {subscription.Id}";
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

            return BaseResponse<PaymentResponseDto>.Ok(MapToDto(payment), "Thanh toán tiền mặt thành công (Đã áp dụng làm tròn tiền).");
        }
        else if (method == "ONLINE_BANKING")
        {
            // --- Luồng Thanh toán Online qua Ngân hàng (Không làm tròn tiền) ---
            // Sinh mã orderCode ngẫu nhiên duy nhất cho PayOS (số nguyên 64-bit)
            long orderCode = new Random().NextInt64(100000000000000, 999999999999999);

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
                // Gọi tới PayOS Gateway để tạo link chuyển khoản ngân hàng
                var (checkoutUrl, qrCode) = await _payOSGateway.CreatePaymentLinkAsync(orderCode, originalAmount, description);

                var responseDto = MapToDto(payment);
                responseDto.PaymentUrl = checkoutUrl;
                responseDto.QrCodeUrl = qrCode;

                return BaseResponse<PaymentResponseDto>.Ok(responseDto, "Tạo liên kết thanh toán chuyển khoản ngân hàng thành công.");
            }
            catch (Exception ex)
            {
                // Nếu lỗi tạo link, đưa trạng thái giao dịch về FAILED
                payment.PaymentStatus = "FAILED";
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangesAsync();

                return BaseResponse<PaymentResponseDto>.Fail("GATEWAY_ERROR", "Lỗi từ cổng thanh toán PayOS: " + ex.Message);
            }
        }

        return BaseResponse<PaymentResponseDto>.Fail("INVALID_PAYMENT_METHOD", "Phương thức thanh toán không được hỗ trợ (chỉ chấp nhận CASH hoặc ONLINE_BANKING).");
    }

    /// <summary>
    /// Xử lý Webhook PayOS phản hồi chuyển khoản thành công.
    /// </summary>
    public async Task<BaseResponse<string>> ProcessWebhookAsync(PayOSWebhookRequest webhookRequest)
    {
        var data = webhookRequest.Data;

        // 1. Sắp xếp các tham số của data theo bảng chữ cái để tạo chuỗi xác thực chữ ký Webhook
        // Theo chuẩn PayOS: amount=value&description=value&orderCode=value&reference=value&status=value
        var dataString = $"amount={(int)data.Amount}&description={data.Description}&orderCode={data.OrderCode}&reference={data.Reference}&status={data.Status}";

        // 2. Xác thực tính toàn vẹn của chữ ký số từ PayOS
        bool isSignatureValid = _payOSGateway.VerifyWebhookSignature(dataString, webhookRequest.Signature);
        if (!isSignatureValid)
        {
            return BaseResponse<string>.Fail("INVALID_SIGNATURE", "Xác thực chữ ký số Webhook thất bại. Dữ liệu bị giả mạo.");
        }

        // 3. Nếu trạng thái giao dịch từ cổng là PAID
        if (data.Status.ToUpper() == "PAID" || webhookRequest.Code == "00")
        {
            // Tìm giao dịch thanh toán trong hệ thống qua orderCode
            var payment = (await _paymentRepository.FindAsync(p => p.OrderCode == data.OrderCode)).FirstOrDefault();

            if (payment == null)
            {
                return BaseResponse<string>.Fail("PAYMENT_NOT_FOUND", $"Không tìm thấy giao dịch tương ứng với mã đơn hàng {data.OrderCode} trong hệ thống.");
            }

            if (payment.PaymentStatus == "PENDING")
            {
                // Cập nhật trạng thái giao dịch
                payment.PaymentStatus = "PAID";
                payment.PaymentTime = DateTime.UtcNow;
                _paymentRepository.Update(payment);
                await _paymentRepository.SaveChangesAsync();

                // Hoàn tất luồng nghiệp vụ tương ứng
                await CompleteBusinessFlowAsync(payment);

                return BaseResponse<string>.Ok("SUCCESS", "Xử lý xác nhận thanh toán Webhook thành công.");
            }
        }

        return BaseResponse<string>.Ok("IGNORED", "Giao dịch không ở trạng thái thành công hoặc đã xử lý từ trước.");
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.Payment.DTOs;
using PBMS.Application.Payment.Interfaces;
using PBMS.Application.Payment.Services;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Application.Revenue.Interfaces;
using PBMS.Domain.Entities;
using Xunit;
using PBMS.Domain.Engine;
using BookingEntity = PBMS.Domain.Entities.Booking;

namespace PBMS.UnitTests
{
    public class PaymentServiceTests
    {
        private readonly IPaymentRepository _paymentRepositoryMock;
        private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepositoryMock;
        private readonly IRepository<BookingEntity> _bookingRepositoryMock;
        private readonly IMonthlySubscriptionRepository _subscriptionRepositoryMock;
        private readonly IRepository<Vehicle> _vehicleRepositoryMock;
        private readonly ICardRepository _cardRepositoryMock;
        private readonly IVNPayGateway _vnpayGatewayMock;
        private readonly IParkingSessionService _sessionServiceMock;
        private readonly IPricingCalculationService _pricingCalculationServiceMock;
        private readonly IConfiguration _configurationMock;
        private readonly IRevenueService _revenueServiceMock;
        private readonly IIncidentRepository _incidentRepositoryMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _paymentRepositoryMock = Substitute.For<IPaymentRepository>();
            _sessionRepositoryMock = Substitute.For<IRepository<PBMS.Domain.Entities.ParkingSession>>();
            _bookingRepositoryMock = Substitute.For<IRepository<BookingEntity>>();
            _subscriptionRepositoryMock = Substitute.For<IMonthlySubscriptionRepository>();
            _vehicleRepositoryMock = Substitute.For<IRepository<Vehicle>>();
            _cardRepositoryMock = Substitute.For<ICardRepository>();
            _vnpayGatewayMock = Substitute.For<IVNPayGateway>();
            _sessionServiceMock = Substitute.For<IParkingSessionService>();
            _pricingCalculationServiceMock = Substitute.For<IPricingCalculationService>();
            _configurationMock = Substitute.For<IConfiguration>();
            _revenueServiceMock = Substitute.For<IRevenueService>();
            _incidentRepositoryMock = Substitute.For<IIncidentRepository>();

            _paymentService = new PaymentService(
                _paymentRepositoryMock,
                _sessionRepositoryMock,
                _bookingRepositoryMock,
                _subscriptionRepositoryMock,
                _vehicleRepositoryMock,
                _cardRepositoryMock,
                _vnpayGatewayMock,
                _sessionServiceMock,
                _pricingCalculationServiceMock,
                _configurationMock,
                _revenueServiceMock,
                _incidentRepositoryMock
            );
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldCancelPendingPayments_WhenCreatingNewPayment()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                SessionId = 1,
                PaymentMethod = "CASH"
            };

            var session = new PBMS.Domain.Entities.ParkingSession
            {
                Id = 1,
                VehicleId = 10,
                SessionStatus = "ACTIVE",
                CheckInTime = DateTime.UtcNow.AddHours(-2),
                CheckOutTime = DateTime.UtcNow
            };

            var vehicle = new Vehicle { Id = 10, VehicleTypeId = 2 };

            var oldPendingPayment = new Payment
            {
                Id = 99,
                SessionId = 1,
                PaymentStatus = "PENDING"
            };

            _sessionRepositoryMock.GetByIdAsync(1).Returns(session);
            _vehicleRepositoryMock.GetByIdAsync(10).Returns(vehicle);

            // Mock finding old pending payments
            _paymentRepositoryMock.FindAsync(Arg.Any<Expression<Func<Payment, bool>>>())
                .Returns(new List<Payment> { oldPendingPayment });

            _pricingCalculationServiceMock.CalculateFeeAsync(2, session.CheckInTime, session.CheckOutTime.Value, session.Id)
                .Returns(new PricingResult { BaseAmount = 20000, IncrementAmount = 0, TotalAmount = 20000 });

            _configurationMock["PaymentSettings:CashRoundingUnit"].Returns("500");
            _configurationMock["PaymentSettings:RoundingThreshold"].Returns("250");

            // Act
            var result = await _paymentService.CreatePaymentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("FAILED", oldPendingPayment.PaymentStatus); // Old payment must be cancelled
            _paymentRepositoryMock.Received(1).Update(oldPendingPayment);
            await _paymentRepositoryMock.Received(1).AddAsync(Arg.Is<Payment>(p => 
                p.SessionId == 1 && 
                p.PaymentMethod == "CASH" && 
                p.PaymentStatus == "PAID"
            ));
        }

        [Fact]
        public async Task ProcessVNPayIPNAsync_ShouldFail_WhenPaymentHasExpired()
        {
            // Arrange
            var vnpayData = new SortedDictionary<string, string>
            {
                { "vnp_SecureHash", "VALID_HASH" },
                { "vnp_TxnRef", "123456" },
                { "vnp_ResponseCode", "00" },
                { "vnp_TransactionStatus", "00" }
            };

            var expiredPayment = new Payment
            {
                Id = 1,
                OrderCode = 123456,
                PaymentStatus = "PENDING"
            };
            // Thiết lập CreatedAt là 16 phút trước
            expiredPayment.CreatedAt = DateTime.UtcNow.AddMinutes(-16);

            _vnpayGatewayMock.VerifySignature(vnpayData, "VALID_HASH").Returns(true);
            _paymentRepositoryMock.FindAsync(Arg.Any<Expression<Func<Payment, bool>>>())
                .Returns(new List<Payment> { expiredPayment });

            // Act
            var result = await _paymentService.ProcessVNPayIPNAsync(vnpayData);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("PAYMENT_EXPIRED", result.ErrorCode);
            Assert.Equal("FAILED", expiredPayment.PaymentStatus); // Phải đánh dấu FAILED
            _paymentRepositoryMock.Received(1).Update(expiredPayment);
        }

        [Fact]
        public async Task ProcessRefundAsync_ShouldRefundSuccessfully_WhenStatusIsRefundPending()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                Amount = 50000,
                PaymentStatus = "REFUND_PENDING",
                PaymentMethod = "ONLINE_BANKING"
            };
            _paymentRepositoryMock.GetByIdAsync(1).Returns(payment);

            // Act
            var result = await _paymentService.ProcessRefundAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("REFUNDED", payment.PaymentStatus);
            Assert.NotNull(payment.PaymentTime);
            _paymentRepositoryMock.Received(1).Update(payment);
            await _paymentRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ProcessRefundAsync_ShouldReturnFail_WhenPaymentNotFound()
        {
            // Arrange
            _paymentRepositoryMock.GetByIdAsync(99).Returns((Payment)null);

            // Act
            var result = await _paymentService.ProcessRefundAsync(99);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.ErrorCode);
        }

        [Fact]
        public async Task ProcessRefundAsync_ShouldReturnFail_WhenStatusIsNotRefundPending()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                Amount = 50000,
                PaymentStatus = "PAID"
            };
            _paymentRepositoryMock.GetByIdAsync(1).Returns(payment);

            // Act
            var result = await _paymentService.ProcessRefundAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("INVALID_PAYMENT_STATUS", result.ErrorCode);
        }
    }
}

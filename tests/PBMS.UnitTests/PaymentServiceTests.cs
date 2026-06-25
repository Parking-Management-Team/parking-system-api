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
using BookingEntity = PBMS.Domain.Entities.Booking;

namespace PBMS.UnitTests
{
    public class PaymentServiceTests
    {
        private readonly IRepository<Payment> _paymentRepositoryMock;
        private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepositoryMock;
        private readonly IRepository<BookingEntity> _bookingRepositoryMock;
        private readonly IMonthlySubscriptionRepository _subscriptionRepositoryMock;
        private readonly IRepository<Vehicle> _vehicleRepositoryMock;
        private readonly ICardRepository _cardRepositoryMock;
        private readonly IVNPayGateway _vnpayGatewayMock;
        private readonly IParkingSessionService _sessionServiceMock;
        private readonly IFeeCalculationService _feeCalculationServiceMock;
        private readonly IConfiguration _configurationMock;
        private readonly IRevenueService _revenueServiceMock;
        private readonly IIncidentRepository _incidentRepositoryMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _paymentRepositoryMock = Substitute.For<IRepository<Payment>>();
            _sessionRepositoryMock = Substitute.For<IRepository<PBMS.Domain.Entities.ParkingSession>>();
            _bookingRepositoryMock = Substitute.For<IRepository<BookingEntity>>();
            _subscriptionRepositoryMock = Substitute.For<IMonthlySubscriptionRepository>();
            _vehicleRepositoryMock = Substitute.For<IRepository<Vehicle>>();
            _cardRepositoryMock = Substitute.For<ICardRepository>();
            _vnpayGatewayMock = Substitute.For<IVNPayGateway>();
            _sessionServiceMock = Substitute.For<IParkingSessionService>();
            _feeCalculationServiceMock = Substitute.For<IFeeCalculationService>();
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
                _feeCalculationServiceMock,
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

            _feeCalculationServiceMock.CalculateFeeAsync(2, session.CheckInTime, session.CheckOutTime.Value)
                .Returns(new PBMS.Application.Pricing.DTOs.FeeCalculationResult { TotalFee = 20000 });

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
    }
}

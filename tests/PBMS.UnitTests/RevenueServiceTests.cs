using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.Revenue.DTOs;
using PBMS.Application.Revenue.Services;
using PBMS.Domain.Entities;
using Xunit;

namespace PBMS.UnitTests
{
    public class RevenueServiceTests
    {
        private readonly IRepository<RevenueStatistic> _revenueStatisticRepositoryMock;
        private readonly IRepository<RevenueStatisticPayment> _revenueStatisticPaymentRepositoryMock;
        private readonly IPaymentRepository _paymentRepositoryMock;
        private readonly IRepository<Building> _buildingRepositoryMock;
        private readonly IRepository<ParkingSession> _sessionRepositoryMock;
        private readonly IRepository<Booking> _bookingRepositoryMock;
        private readonly IRepository<Vehicle> _vehicleRepositoryMock;
        private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
        private readonly RevenueService _revenueService;

        public RevenueServiceTests()
        {
            _revenueStatisticRepositoryMock = Substitute.For<IRepository<RevenueStatistic>>();
            _revenueStatisticPaymentRepositoryMock = Substitute.For<IRepository<RevenueStatisticPayment>>();
            _paymentRepositoryMock = Substitute.For<IPaymentRepository>();
            _buildingRepositoryMock = Substitute.For<IRepository<Building>>();
            _sessionRepositoryMock = Substitute.For<IRepository<ParkingSession>>();
            _bookingRepositoryMock = Substitute.For<IRepository<Booking>>();
            _vehicleRepositoryMock = Substitute.For<IRepository<Vehicle>>();
            _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();

            _revenueService = new RevenueService(
                _revenueStatisticRepositoryMock,
                _revenueStatisticPaymentRepositoryMock,
                _paymentRepositoryMock,
                _buildingRepositoryMock,
                _sessionRepositoryMock,
                _bookingRepositoryMock,
                _vehicleRepositoryMock,
                _vehicleTypeRepositoryMock
            );
        }

        [Fact]
        public async Task UpdateRevenueAfterPaymentAsync_ShouldReturnCompletedTask()
        {
            // Act & Assert
            await _revenueService.UpdateRevenueAfterPaymentAsync(1);
            // Verify no calls were made to write repositories
            await _revenueStatisticRepositoryMock.DidNotReceiveWithAnyArgs().AddAsync(null!);
        }

        [Fact]
        public async Task GetRevenueStatisticsAsync_ShouldReturnCorrectRealtimeAggregations()
        {
            // Arrange
            var filter = new RevenueFilterDto
            {
                BuildingId = 1,
                PeriodType = "DAILY",
                StartDate = new DateOnly(2026, 7, 13),
                EndDate = new DateOnly(2026, 7, 14)
            };

            var building = new Building { Id = 1, Name = "Building A" };
            var vehicleType1 = new VehicleType { Id = 2, TypeName = "Car" };

            _buildingRepositoryMock.GetAllAsync().Returns(new List<Building> { building });
            _vehicleTypeRepositoryMock.GetAllAsync().Returns(new List<VehicleType> { vehicleType1 });

            // Payment 1: July 13th 18:00 UTC (which is July 14th 01:00 Vietnam time UTC+7)
            var p1 = new Payment
            {
                Id = 101,
                Amount = 50000,
                PaymentStatus = "PAID",
                PaymentTime = new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc),
                SessionId = 10,
                Session = new ParkingSession
                {
                    Id = 10,
                    BuildingId = 1,
                    Vehicle = new Vehicle { VehicleTypeId = 2 }
                }
            };

            // Payment 2: July 13th 10:00 UTC (which is July 13th 17:00 Vietnam time UTC+7)
            var p2 = new Payment
            {
                Id = 102,
                Amount = 30000,
                PaymentStatus = "PAID",
                PaymentTime = new DateTime(2026, 7, 13, 10, 0, 0, DateTimeKind.Utc),
                BookingId = 20,
                Booking = new Booking
                {
                    Id = 20,
                    BuildingId = 1,
                    VehicleTypeId = 2
                }
            };

            _paymentRepositoryMock.GetPaidPaymentsAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                .Returns(new List<Payment> { p1, p2 });

            // Act
            var result = await _revenueService.GetRevenueStatisticsAsync(filter, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Items.Any());

            // Verify mapping and grouping
            var july13Group = result.Items.FirstOrDefault(x => x.StartDate == new DateOnly(2026, 7, 13) && x.VehicleTypeId == 2);
            Assert.NotNull(july13Group);
            Assert.Equal(30000, july13Group.TotalRevenue);

            var july14Group = result.Items.FirstOrDefault(x => x.StartDate == new DateOnly(2026, 7, 14) && x.VehicleTypeId == 2);
            Assert.NotNull(july14Group);
            Assert.Equal(50000, july14Group.TotalRevenue);
        }

        [Fact]
        public async Task GetRevenueStatisticByIdAsync_ShouldDecodeAndReturnDetailCorrectly()
        {
            // Arrange
            // Let's encode a specific ID: Building 1, July 14th 2026, VehicleType 2, DAILY
            // DecodeId should give these exact parameters back.
            var p1 = new Payment
            {
                Id = 101,
                Amount = 50000,
                PaymentStatus = "PAID",
                PaymentTime = new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc), // Vietnam: July 14th 01:00
                SessionId = 10,
                Session = new ParkingSession
                {
                    Id = 10,
                    BuildingId = 1,
                    Vehicle = new Vehicle { VehicleTypeId = 2 }
                }
            };

            _paymentRepositoryMock.GetPaidPaymentsAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                .Returns(new List<Payment> { p1 });

            var building = new Building { Id = 1, Name = "Building A" };
            var vehicleType = new VehicleType { Id = 2, TypeName = "Car" };

            _buildingRepositoryMock.GetByIdAsync(1).Returns(building);
            _vehicleTypeRepositoryMock.GetByIdAsync(2).Returns(vehicleType);

            // Encode parameters
            // BuildingId: 1, Date: 2026-07-14, VehicleTypeId: 2, PeriodType: DAILY
            // Let's call the private/static method via reflection or just compute the exact expected ID.
            // bId = 1 << 22 = 4194304
            // vId = 2 << 17 = 262144
            // pType = 0 << 15 = 0
            // yearOffset = (2026-2024) = 2 << 9 = 1024
            // month = 7 << 5 = 224
            // day = 14
            // Expected Id = 4194304 + 262144 + 1024 + 224 + 14 = 4457710
            int testId = 4457710;

            // Act
            var result = await _revenueService.GetRevenueStatisticByIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BuildingId);
            Assert.Equal(new DateOnly(2026, 7, 14), result.StartDate);
            Assert.Equal(2, result.VehicleTypeId);
            Assert.Equal(50000, result.TotalRevenue);
            Assert.Single(result.Payments);
            Assert.Equal(101, result.Payments[0].PaymentId);
        }
    }
}

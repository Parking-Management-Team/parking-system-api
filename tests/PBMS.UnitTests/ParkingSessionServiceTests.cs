using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Services;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;

namespace PBMS.UnitTests;

public class ParkingSessionServiceTests
{
    private readonly IParkingSessionRepository _sessionRepositoryMock;
    private readonly IRepository<VehicleEntity> _vehicleRepositoryMock;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepositoryMock;
    private readonly IRepository<Booking> _bookingRepositoryMock;
    private readonly IFeeCalculationService _feeCalculationServiceMock;
    private readonly ICardRepository _cardRepositoryMock;
    private readonly IMonthlySubscriptionRepository _subscriptionRepositoryMock;
    private readonly IParkingSlotRepository _parkingSlotRepositoryMock;
    private readonly ParkingSessionService _service;

    public ParkingSessionServiceTests()
    {
        _sessionRepositoryMock = Substitute.For<IParkingSessionRepository>();
        _vehicleRepositoryMock = Substitute.For<IRepository<VehicleEntity>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleTypeEntity>>();
        _bookingRepositoryMock = Substitute.For<IRepository<Booking>>();
        _feeCalculationServiceMock = Substitute.For<IFeeCalculationService>();
        _cardRepositoryMock = Substitute.For<ICardRepository>();
        _subscriptionRepositoryMock = Substitute.For<IMonthlySubscriptionRepository>();
        _parkingSlotRepositoryMock = Substitute.For<IParkingSlotRepository>();

        _service = new ParkingSessionService(
            _sessionRepositoryMock,
            _vehicleRepositoryMock,
            _vehicleTypeRepositoryMock,
            _bookingRepositoryMock,
            _feeCalculationServiceMock,
            _cardRepositoryMock,
            _subscriptionRepositoryMock,
            _parkingSlotRepositoryMock
        );
    }

    [Fact]
    public async Task CheckInAsync_ShouldBypassAvailableCheck_WhenCardIsAssignedToActiveSubscription()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "M-CARD-1",
            VehicleTypeId = 1,
            BuildingId = 10,
            StaffId = 5
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = VehicleTypeEntity.MotorcycleTypeName };
        var card = new Card { Id = 100, CardCode = "M-CARD-1", CardType = "MONTHLY", CardStatus = CardStatus.Assigned.ToString() };
        var vehicle = new VehicleEntity { Id = 200, LicensePlate = "29A-12345", VehicleTypeId = 1 };
        var activeSub = new MonthlySubscription
        {
            Id = 500,
            VehicleId = 200,
            BuildingId = 10,
            AssignedCardId = 100,
            ActivatedAt = DateTime.UtcNow.AddDays(-5),
            ExpiredAt = DateTime.UtcNow.AddDays(25),
            Vehicle = vehicle,
            MonthlySubscriptionStatus = "ACTIVE"
        };
        var zone = new Zone { Id = 9, Code = "M-ZONE", Floor = new Floor { BuildingId = 10 } };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("M-CARD-1").Returns(card);
        _subscriptionRepositoryMock.GetActiveSubscriptionByCardIdAsync(100).Returns(activeSub);
        _sessionRepositoryMock.GetVehicleByLicensePlateAsync("29A-12345").Returns(vehicle);
        _sessionRepositoryMock.HasActiveSessionForVehicleAsync(200).Returns(false);
        _sessionRepositoryMock.FindAvailableZoneAsync(1, 10).Returns(zone);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(500, result.Data.MonthlySubscriptionId);
        Assert.Equal("Assigned", card.CardStatus); // Trạng thái thẻ tháng giữ nguyên Assigned
    }

    [Fact]
    public async Task StartCheckoutAsync_ShouldCompleteImmediately_WhenMonthlySubscriptionIsValid()
    {
        // Arrange
        int sessionId = 1;
        var request = new StartCheckoutRequest { CheckOutTime = DateTime.UtcNow };

        var session = new ParkingSession
        {
            Id = sessionId,
            VehicleId = 2,
            CardId = 3,
            SessionStatus = "ACTIVE",
            CheckInTime = DateTime.UtcNow.AddHours(-2),
            MonthlySubscriptionId = 500
        };

        var subscription = new MonthlySubscription
        {
            Id = 500,
            ExpiredAt = DateTime.UtcNow.AddDays(10) // Còn hạn
        };

        _sessionRepositoryMock.GetByIdAsync(sessionId).Returns(session);
        _subscriptionRepositoryMock.GetByIdAsync(500).Returns(subscription);

        // Act
        var result = await _service.StartCheckoutAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("COMPLETED", result.Data.SessionStatus); // Hoàn tất check-out ngay lập tức
    }

    [Fact]
    public async Task StartCheckoutAsync_ShouldNotCompleteImmediately_WhenMonthlySubscriptionIsExpired()
    {
        // Arrange
        int sessionId = 1;
        var checkOutTime = DateTime.UtcNow;
        var request = new StartCheckoutRequest { CheckOutTime = checkOutTime };

        var session = new ParkingSession
        {
            Id = sessionId,
            VehicleId = 2,
            CardId = 3,
            SessionStatus = "ACTIVE",
            CheckInTime = DateTime.UtcNow.AddHours(-10),
            MonthlySubscriptionId = 500
        };

        var expiredAt = DateTime.UtcNow.AddHours(-5); // Hết hạn trước lúc check-out
        var subscription = new MonthlySubscription
        {
            Id = 500,
            ExpiredAt = expiredAt
        };

        _sessionRepositoryMock.GetByIdAsync(sessionId).Returns(session);
        _subscriptionRepositoryMock.GetByIdAsync(500).Returns(subscription);

        // Act
        var result = await _service.StartCheckoutAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("ACTIVE", result.Data.SessionStatus); // Giữ ACTIVE chờ thanh toán phí overtime
    }
}

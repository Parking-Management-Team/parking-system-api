using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Services;
using PBMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace PBMS.UnitTests;

public class ParkingSessionServiceTests
{
    private readonly IRepository<ParkingSession> _sessionRepoMock;
    private readonly ICardRepository _cardRepoMock;
    private readonly IMonthlySubscriptionRepository _monthlySubRepoMock;
    private readonly ParkingSessionService _service;

    public ParkingSessionServiceTests()
    {
        _sessionRepoMock = Substitute.For<IRepository<ParkingSession>>();
        _cardRepoMock = Substitute.For<ICardRepository>();
        _monthlySubRepoMock = Substitute.For<IMonthlySubscriptionRepository>();

        _service = new ParkingSessionService(
            _sessionRepoMock,
            _cardRepoMock,
            _monthlySubRepoMock
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenCardIsLost()
    {
        // Arrange
        var request = new CreateParkingSessionRequest
        {
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-12345"
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-001",
            CardStatus = "Lost"
        };

        _cardRepoMock.GetByIdAsync(1).Returns(card);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("CARD_LOST", result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenCardIsBlocked()
    {
        // Arrange
        var request = new CreateParkingSessionRequest
        {
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-12345"
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-001",
            CardStatus = "Blocked"
        };

        _cardRepoMock.GetByIdAsync(1).Returns(card);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("CARD_BLOCKED", result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenCardAlreadyActiveInDb()
    {
        // Arrange
        var request = new CreateParkingSessionRequest
        {
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-12345"
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-001",
            CardStatus = "Active"
        };

        _cardRepoMock.GetByIdAsync(1).Returns(card);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("CARD_IN_ACTIVE_SESSION", result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenLicensePlateMismatch_WithMonthlyCard()
    {
        // Arrange
        var request = new CreateParkingSessionRequest
        {
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-55555" // Mismatched license plate
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-MONTHLY",
            CardStatus = "Available",
            CardType = "MONTHLY"
        };

        var subscription = new MonthlySubscription
        {
            Id = 5,
            AccountId = 20,
            VehicleId = 10,
            BuildingId = 100,
            MonthlySubscriptionStatus = "ACTIVE",
            ActivatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiredAt = DateTime.UtcNow.AddDays(10),
            Vehicle = new Vehicle { Id = 10, LicensePlate = "29A-12345" }
        };

        _cardRepoMock.GetByIdAsync(1).Returns(card);
        _monthlySubRepoMock.GetActiveSubscriptionByCardCodeAsync("CARD-MONTHLY").Returns(subscription);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("LICENSE_PLATE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_AndSetCardStatusToActive_WhenMonthlyCardIsValid()
    {
        // Arrange
        var request = new CreateParkingSessionRequest
        {
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-12345"
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-MONTHLY",
            CardStatus = "Available",
            CardType = "MONTHLY"
        };

        var subscription = new MonthlySubscription
        {
            Id = 5,
            AccountId = 20,
            VehicleId = 10,
            BuildingId = 100,
            MonthlySubscriptionStatus = "ACTIVE",
            ActivatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiredAt = DateTime.UtcNow.AddDays(10),
            AssignedSlotId = 77, // Auto slot assignment
            Vehicle = new Vehicle { Id = 10, LicensePlate = "29A-12345" }
        };

        _cardRepoMock.GetByIdAsync(1).Returns(card);
        _monthlySubRepoMock.GetActiveSubscriptionByCardCodeAsync("CARD-MONTHLY").Returns(subscription);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.MonthlySubscriptionId);
        Assert.Equal(77, result.Data.SlotId);
        Assert.Equal("ACTIVE", result.Data.SessionStatus);

        // Verify card status updated to Active in DB
        Assert.Equal("Active", card.CardStatus);
        _cardRepoMock.Received(1).Update(card);
        await _sessionRepoMock.Received(1).AddAsync(Arg.Any<ParkingSession>());
        await _sessionRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CompleteAsync_ShouldCompleteSession_AndSetCardStatusToAvailable()
    {
        // Arrange
        int sessionId = 123;
        var session = new ParkingSession
        {
            Id = sessionId,
            CardId = 1,
            VehicleId = 10,
            BuildingId = 100,
            LicensePlateIn = "29A-12345",
            SessionStatus = "ACTIVE"
        };

        var card = new Card
        {
            Id = 1,
            CardCode = "CARD-001",
            CardStatus = "Active"
        };

        _sessionRepoMock.GetByIdAsync(sessionId).Returns(session);
        _cardRepoMock.GetByIdAsync(1).Returns(card);

        // Act
        var result = await _service.CompleteAsync(sessionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("COMPLETED", result.Data.SessionStatus);

        // Verify card status reverted to Available in DB
        Assert.Equal("Available", card.CardStatus);
        _cardRepoMock.Received(1).Update(card);
        _sessionRepoMock.Received(1).Update(session);
        await _sessionRepoMock.Received(1).SaveChangesAsync();
    }
}

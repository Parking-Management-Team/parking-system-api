using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Services;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PBMS.UnitTests;

public class MonthlySubscriptionServiceTests
{
    private readonly IMonthlySubscriptionRepository _monthlySubRepoMock;
    private readonly ICardRepository _cardRepoMock;
    private readonly IVehicleRepository _vehicleRepoMock;
    private readonly IAccountRepository _accountRepoMock;
    private readonly IBuildingRepository _buildingRepoMock;
    private readonly MonthlySubscriptionService _service;

    public MonthlySubscriptionServiceTests()
    {
        _monthlySubRepoMock = Substitute.For<IMonthlySubscriptionRepository>();
        _cardRepoMock = Substitute.For<ICardRepository>();
        _vehicleRepoMock = Substitute.For<IVehicleRepository>();
        _accountRepoMock = Substitute.For<IAccountRepository>();
        _buildingRepoMock = Substitute.For<IBuildingRepository>();

        _service = new MonthlySubscriptionService(
            _monthlySubRepoMock,
            _cardRepoMock,
            _vehicleRepoMock,
            _accountRepoMock,
            _buildingRepoMock
        );
    }

    [Fact]
    public async Task RegisterSubscriptionAsync_ShouldSucceed_WhenValid()
    {
        // Arrange
        var request = new CreateMonthlySubscriptionRequest
        {
            AccountId = 1,
            VehicleId = 2,
            BuildingId = 3,
            MonthlyPrice = 100000m
        };

        _accountRepoMock.GetByIdAsync(1).Returns(new Account { Id = 1 });
        _vehicleRepoMock.GetByIdAsync(2).Returns(new Vehicle { Id = 2 });
        _buildingRepoMock.GetByIdAsync(3).Returns(new Building { Id = 3 });
        _monthlySubRepoMock.GetActiveSubscriptionByVehicleIdAsync(2).Returns((MonthlySubscription?)null);

        // Act
        var result = await _service.RegisterSubscriptionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PENDING", result.MonthlySubscriptionStatus);
        Assert.Equal(100000m, result.MonthlyPrice);

        await _monthlySubRepoMock.Received(1).AddAsync(Arg.Is<MonthlySubscription>(s => 
            s.AccountId == 1 && s.VehicleId == 2 && s.BuildingId == 3 && s.MonthlySubscriptionStatus == "PENDING"
        ));
        await _monthlySubRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterSubscriptionAsync_ShouldThrow_WhenActiveSubscriptionExists()
    {
        // Arrange
        var request = new CreateMonthlySubscriptionRequest
        {
            AccountId = 1,
            VehicleId = 2,
            BuildingId = 3,
            MonthlyPrice = 100000m
        };

        _accountRepoMock.GetByIdAsync(1).Returns(new Account { Id = 1 });
        _vehicleRepoMock.GetByIdAsync(2).Returns(new Vehicle { Id = 2 });
        _buildingRepoMock.GetByIdAsync(3).Returns(new Building { Id = 3 });
        _monthlySubRepoMock.GetActiveSubscriptionByVehicleIdAsync(2).Returns(new MonthlySubscription { Id = 99, MonthlySubscriptionStatus = "ACTIVE" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.RegisterSubscriptionAsync(request)
        );

        Assert.Equal("ACTIVE_SUBSCRIPTION_EXISTS", exception.ErrorCode);
        await _monthlySubRepoMock.DidNotReceive().AddAsync(Arg.Any<MonthlySubscription>());
    }

    [Fact]
    public async Task ActivateSubscriptionAsync_ShouldSucceed_WhenCardIsAvailable()
    {
        // Arrange
        int subId = 5;
        string cardCode = "CARD-NEW";
        var sub = new MonthlySubscription
        {
            Id = subId,
            MonthlySubscriptionStatus = "PENDING"
        };
        var card = new Card
        {
            Id = 10,
            CardCode = cardCode,
            CardStatus = "Available"
        };

        _monthlySubRepoMock.GetByIdAsync(subId).Returns(sub);
        _cardRepoMock.GetByCardCodeAsync(cardCode).Returns(card);
        _monthlySubRepoMock.IsCardAssignedToActiveSubscriptionAsync(10).Returns(false);

        // Act
        var result = await _service.ActivateSubscriptionAsync(subId, cardCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ACTIVE", sub.MonthlySubscriptionStatus);
        Assert.Equal(10, sub.AssignedCardId);
        Assert.NotNull(sub.ActivatedAt);
        Assert.NotNull(sub.ExpiredAt);

        _monthlySubRepoMock.Received(1).Update(sub);
        await _monthlySubRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ActivateSubscriptionAsync_ShouldThrow_WhenCardIsNotAvailable()
    {
        // Arrange
        int subId = 5;
        string cardCode = "CARD-BUSY";
        var sub = new MonthlySubscription
        {
            Id = subId,
            MonthlySubscriptionStatus = "PENDING"
        };
        var card = new Card
        {
            Id = 10,
            CardCode = cardCode,
            CardStatus = "Active" // Card is already active in a session
        };

        _monthlySubRepoMock.GetByIdAsync(subId).Returns(sub);
        _cardRepoMock.GetByCardCodeAsync(cardCode).Returns(card);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ActivateSubscriptionAsync(subId, cardCode)
        );

        Assert.Equal("CARD_NOT_AVAILABLE", exception.ErrorCode);
        _monthlySubRepoMock.DidNotReceive().Update(Arg.Any<MonthlySubscription>());
    }

    [Fact]
    public async Task ReportLostCardAsync_ShouldSetCardToLost_AndUnassignIt()
    {
        // Arrange
        int subId = 5;
        var sub = new MonthlySubscription
        {
            Id = subId,
            AssignedCardId = 10,
            MonthlySubscriptionStatus = "ACTIVE"
        };
        var card = new Card
        {
            Id = 10,
            CardCode = "CARD-001",
            CardStatus = "Available"
        };

        _monthlySubRepoMock.GetByIdAsync(subId).Returns(sub);
        _cardRepoMock.GetByIdAsync(10).Returns(card);

        // Act
        var result = await _service.ReportLostCardAsync(subId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(sub.AssignedCardId);
        Assert.Equal("Lost", card.CardStatus);

        _cardRepoMock.Received(1).Update(card);
        _monthlySubRepoMock.Received(1).Update(sub);
        await _monthlySubRepoMock.Received(1).SaveChangesAsync();
    }
}

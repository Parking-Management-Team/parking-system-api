using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.MonthlyCard.DTOs;
using PBMS.Application.MonthlyCard.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PBMS.UnitTests;

public class MonthlySubscriptionServiceTests
{
    private readonly IMonthlySubscriptionRepository _subscriptionRepositoryMock;
    private readonly ICardRepository _cardRepositoryMock;
    private readonly IBuildingRepository _buildingRepositoryMock;
    private readonly IRepository<Vehicle> _vehicleRepositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
    private readonly IParkingSlotRepository _parkingSlotRepositoryMock;
    private readonly IRepository<Account> _accountRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly MonthlySubscriptionService _service;

    public MonthlySubscriptionServiceTests()
    {
        _subscriptionRepositoryMock = Substitute.For<IMonthlySubscriptionRepository>();
        _cardRepositoryMock = Substitute.For<ICardRepository>();
        _buildingRepositoryMock = Substitute.For<IBuildingRepository>();
        _vehicleRepositoryMock = Substitute.For<IRepository<Vehicle>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();
        _parkingSlotRepositoryMock = Substitute.For<IParkingSlotRepository>();
        _accountRepositoryMock = Substitute.For<IRepository<Account>>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _service = new MonthlySubscriptionService(
            _subscriptionRepositoryMock,
            _cardRepositoryMock,
            _buildingRepositoryMock,
            _vehicleRepositoryMock,
            _vehicleTypeRepositoryMock,
            _parkingSlotRepositoryMock,
            _accountRepositoryMock,
            _unitOfWorkMock
        );
    }

    [Fact]
    public async Task RegisterSubscriptionAsync_ShouldCreateSubscription_WhenRequestIsValidForMotorcycle()
    {
        // Arrange
        var request = new CreateSubscriptionRequest
        {
            AccountId = 1,
            VehicleId = 2,
            BuildingId = 3,
            AssignedCardId = 4
        };

        var account = new Account { Id = 1 };
        var vehicle = new Vehicle { Id = 2, VehicleTypeId = 10 };
        var building = new Building { Id = 3 };
        var vehicleType = new VehicleType { Id = 10, TypeName = VehicleType.MotorcycleTypeName };
        var card = new Card { Id = 4, CardCode = "M-CARD-1", CardType = "MONTHLY", CardStatus = CardStatus.Available.ToString() };

        _accountRepositoryMock.GetByIdAsync(1).Returns(account);
        _vehicleRepositoryMock.GetByIdAsync(2).Returns(vehicle);
        _buildingRepositoryMock.GetByIdAsync(3).Returns(building);
        _vehicleTypeRepositoryMock.GetByIdAsync(10).Returns(vehicleType);
        _subscriptionRepositoryMock.HasOverlapSubscriptionAsync(2).Returns(false);
        _cardRepositoryMock.GetByIdAsync(4).Returns(card);

        _subscriptionRepositoryMock.GetActiveAndPendingMotorcycleSubscriptionsCountAsync(3).Returns(5);
        _buildingRepositoryMock.GetTotalMotorcycleCapacityAsync(3).Returns(10);

        // Act
        var result = await _service.RegisterSubscriptionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(120000m, result.MonthlyPrice);
        Assert.Equal("PENDING", result.MonthlySubscriptionStatus);

        await _subscriptionRepositoryMock.Received(1).AddAsync(Arg.Is<MonthlySubscription>(ms =>
            ms.AccountId == 1 &&
            ms.VehicleId == 2 &&
            ms.BuildingId == 3 &&
            ms.AssignedCardId == 4 &&
            ms.MonthlyPrice == 120000m &&
            ms.MonthlySubscriptionStatus == "PENDING"
        ));
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterSubscriptionAsync_ShouldThrowException_WhenMotorcycleCapacityIsFull()
    {
        // Arrange
        var request = new CreateSubscriptionRequest { AccountId = 1, VehicleId = 2, BuildingId = 3 };
        var account = new Account { Id = 1 };
        var vehicle = new Vehicle { Id = 2, VehicleTypeId = 10 };
        var building = new Building { Id = 3 };
        var vehicleType = new VehicleType { Id = 10, TypeName = VehicleType.MotorcycleTypeName };

        _accountRepositoryMock.GetByIdAsync(1).Returns(account);
        _vehicleRepositoryMock.GetByIdAsync(2).Returns(vehicle);
        _buildingRepositoryMock.GetByIdAsync(3).Returns(building);
        _vehicleTypeRepositoryMock.GetByIdAsync(10).Returns(vehicleType);
        _subscriptionRepositoryMock.HasOverlapSubscriptionAsync(2).Returns(false);

        _subscriptionRepositoryMock.GetActiveAndPendingMotorcycleSubscriptionsCountAsync(3).Returns(10);
        _buildingRepositoryMock.GetTotalMotorcycleCapacityAsync(3).Returns(10);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.RegisterSubscriptionAsync(request));
        Assert.Equal("CAPACITY_FULL", ex.ErrorCode);
    }

    [Fact]
    public async Task RegisterSubscriptionAsync_ShouldAssignSlot_WhenRequestIsValidForCar()
    {
        // Arrange
        var request = new CreateSubscriptionRequest { AccountId = 1, VehicleId = 2, BuildingId = 3 };
        var account = new Account { Id = 1 };
        var vehicle = new Vehicle { Id = 2, VehicleTypeId = 20 };
        var building = new Building { Id = 3 };
        var vehicleType = new VehicleType { Id = 20, TypeName = VehicleType.CarTypeName };
        var slot = new ParkingSlot { Id = 50, Code = "SLOT-M1" };

        _accountRepositoryMock.GetByIdAsync(1).Returns(account);
        _vehicleRepositoryMock.GetByIdAsync(2).Returns(vehicle);
        _buildingRepositoryMock.GetByIdAsync(3).Returns(building);
        _vehicleTypeRepositoryMock.GetByIdAsync(20).Returns(vehicleType);
        _subscriptionRepositoryMock.HasOverlapSubscriptionAsync(2).Returns(false);
        _parkingSlotRepositoryMock.FindAvailableMonthlySlotAsync(3, 20).Returns(slot);

        // Act
        var result = await _service.RegisterSubscriptionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500000m, result.MonthlyPrice);
        Assert.Equal(50, result.AssignedSlotId);
    }
}

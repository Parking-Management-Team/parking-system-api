using AutoMapper;
using NSubstitute;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using Xunit;

namespace PBMS.UnitTests.ParkingStructure;

using BookingEntity = PBMS.Domain.Entities.Booking;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;

public class BuildingServiceTests
{
    private readonly IBuildingRepository _buildingRepositoryMock;
    private readonly IRepository<Floor> _floorRepositoryMock;
    private readonly IZoneRepository _zoneRepositoryMock;
    private readonly IParkingSlotRepository _slotRepositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly IRepository<ParkingSessionEntity> _sessionRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly BuildingService _buildingService;

    public BuildingServiceTests()
    {
        _buildingRepositoryMock = Substitute.For<IBuildingRepository>();
        _floorRepositoryMock = Substitute.For<IRepository<Floor>>();
        _zoneRepositoryMock = Substitute.For<IZoneRepository>();
        _slotRepositoryMock = Substitute.For<IParkingSlotRepository>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _sessionRepositoryMock = Substitute.For<IRepository<ParkingSessionEntity>>();
        _mapperMock = Substitute.For<IMapper>();

        _buildingService = new BuildingService(
            _buildingRepositoryMock, 
            _floorRepositoryMock, 
            _zoneRepositoryMock, 
            _slotRepositoryMock, 
            _vehicleTypeRepositoryMock,
            _bookingRepositoryMock,
            _sessionRepositoryMock,
            _mapperMock);
    }

    [Fact]
    public async Task CreateBuildingAsync_ShouldReturnBuildingDto_WhenRequestIsValid()
    {
        // Arrange
        var request = new BuildingCreateRequest
        {
            Code = "BLD-01",
            Name = "Building 1",
            Address = "123 Street",
            TotalFloor = 5
        };

        var buildingDto = new BuildingDto { Id = 1, Code = "BLD-01", Name = "Building 1" };

        _buildingRepositoryMock.BuildingCodeExistsAsync(request.Code).Returns(false);
        _mapperMock.Map<BuildingDto>(Arg.Any<Building>()).Returns(buildingDto);

        // Act
        var result = await _buildingService.CreateBuildingAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Code, result.Code);
        await _buildingRepositoryMock.Received(1).AddAsync(Arg.Any<Building>());
    }

    [Fact]
    public async Task CreateBuildingAsync_ShouldThrowValidationException_WhenCodeExists()
    {
        // Arrange
        var request = new BuildingCreateRequest { Code = "EXISTING", Name = "Building" };
        _buildingRepositoryMock.BuildingCodeExistsAsync(request.Code).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _buildingService.CreateBuildingAsync(request));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_ShouldReturnBuildingDto_WhenExists()
    {
        // Arrange
        int id = 1;
        var building = new Building { Id = id, Code = "BLD-01" };
        var buildingDto = new BuildingDto { Id = id, Code = "BLD-01" };

        _buildingRepositoryMock.GetByIdAsync(id).Returns(building);
        _mapperMock.Map<BuildingDto>(building).Returns(buildingDto);

        // Act
        var result = await _buildingService.GetBuildingByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task UpdateBuildingAsync_ShouldUpdateAndReturnDto_WhenValid()
    {
        // Arrange
        int id = 1;
        var request = new BuildingUpdateRequest { Code = "NEW-CODE", Name = "New Name", Status = BuildingStatus.Active, TotalFloor = 2 };
        var existingBuilding = new Building { Id = id, Code = "OLD-CODE", Floors = new List<Floor> { new Floor { Id = 1 } } };
        var updatedDto = new BuildingDto { Id = id, Code = "NEW-CODE" };

        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(existingBuilding);
        _buildingRepositoryMock.BuildingCodeExistsAsync(request.Code).Returns(false);
        _mapperMock.Map<BuildingDto>(existingBuilding).Returns(updatedDto);

        // Act
        var result = await _buildingService.UpdateBuildingAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Code, result.Code);
        _buildingRepositoryMock.Received(1).Update(existingBuilding);
    }

    [Fact]
    public async Task UpdateBuildingAsync_ShouldThrowValidationException_WhenDecreaseTotalFloorBelowCurrentFloorCount()
    {
        // Arrange
        int id = 1;
        var request = new BuildingUpdateRequest { Code = "SAME-CODE", Name = "Building", Status = BuildingStatus.Active, TotalFloor = 1 };
        var existingBuilding = new Building 
        { 
            Id = id, 
            Code = "SAME-CODE", 
            Floors = new List<Floor> { new Floor { Id = 1 }, new Floor { Id = 2 } } // Has 2 registered floors
        };

        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(existingBuilding);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _buildingService.UpdateBuildingAsync(id, request));
        Assert.Contains("Cannot decrease total floors below the currently registered floor count", exception.Message);
        _buildingRepositoryMock.DidNotReceive().Update(Arg.Any<Building>());
    }

    [Fact]
    public async Task UpdateBuildingAsync_ShouldUpdate_WhenTotalFloorIsGreaterOrEqualToCurrentFloorCount()
    {
        // Arrange
        int id = 1;
        var request = new BuildingUpdateRequest { Code = "NEW-CODE", Name = "New Name", Status = BuildingStatus.Active, TotalFloor = 2 };
        var existingBuilding = new Building 
        { 
            Id = id, 
            Code = "OLD-CODE", 
            Floors = new List<Floor> { new Floor { Id = 1 }, new Floor { Id = 2 } } // Has 2 registered floors
        };
        var updatedDto = new BuildingDto { Id = id, Code = "NEW-CODE" };

        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(existingBuilding);
        _buildingRepositoryMock.BuildingCodeExistsAsync(request.Code).Returns(false);
        _mapperMock.Map<BuildingDto>(existingBuilding).Returns(updatedDto);

        // Act
        var result = await _buildingService.UpdateBuildingAsync(id, request);

        // Assert
        Assert.NotNull(result);
        _buildingRepositoryMock.Received(1).Update(existingBuilding);
    }

    [Fact]
    public async Task DeleteBuildingAsync_ShouldRemove_WhenNoFloorsExist()
    {
        // Arrange
        int id = 1;
        var building = new Building { Id = id, Code = "BLD-01", Floors = new List<Floor>() };
        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(building);

        // Act
        await _buildingService.DeleteBuildingAsync(id);

        // Assert
        await _buildingRepositoryMock.Received(1).RemoveAsync(building);
    }

    [Fact]
    public async Task DeleteBuildingAsync_ShouldThrowValidationException_WhenFloorsExist()
    {
        // Arrange
        int id = 1;
        var building = new Building 
        { 
            Id = id, 
            Code = "BLD-01", 
            Floors = new List<Floor> { new Floor { Id = 1 } } 
        };
        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(building);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _buildingService.DeleteBuildingAsync(id));
        Assert.Contains("contains floors", exception.Message);
        await _buildingRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Building>());
    }

    [Fact]
    public async Task GetAvailableCapacityByTimeframeAsync_ShouldCalculateCorrectly()
    {
        // Arrange
        int id = 1;
        var building = new Building { Id = id, Code = "BLD-01" };
        _buildingRepositoryMock.GetByIdAsync(id).Returns(building);

        var vehicleType = new VehicleType { Id = 1, TypeName = "Car", BufferRatio = 10 };
        _vehicleTypeRepositoryMock.GetAllAsync().Returns(new List<VehicleType> { vehicleType });

        _buildingRepositoryMock.GetTotalGeneralCapacityAsync(id, 1).Returns(10);
        _sessionRepositoryMock.CountAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSessionEntity, bool>>>()).Returns(2);
        _bookingRepositoryMock.GetActiveBookingsCountAsync(id, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(1);

        var start = DateTime.UtcNow;
        var end = start.AddHours(4);

        // Act
        var result = await _buildingService.GetAvailableCapacityByTimeframeAsync(id, start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.BuildingId);
        Assert.Single(result.VehicleTypeCapacities);
        
        var cap = result.VehicleTypeCapacities[0];
        Assert.Equal(1, cap.VehicleTypeId);
        Assert.Equal("Car", cap.VehicleTypeName);
        Assert.Equal(10, cap.TotalCapacity);
        Assert.Equal(1, cap.BufferSlots); // 10 * 0.1 = 1
        Assert.Equal(9, cap.EffectiveCapacity); // 10 - 1 = 9
        Assert.Equal(2, cap.ActiveSessions);
        Assert.Equal(1, cap.ReservedBookings);
        Assert.Equal(6, cap.AvailableCapacity); // 9 - (2 + 1) = 6
    }
}

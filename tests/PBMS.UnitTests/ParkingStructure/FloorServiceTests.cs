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

public class FloorServiceTests
{
    private readonly IFloorRepository _floorRepositoryMock;
    private readonly IBuildingRepository _buildingRepositoryMock;
    private readonly IZoneRepository _zoneRepositoryMock;
    private readonly IParkingSlotRepository _slotRepositoryMock;
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly FloorService _floorService;

    public FloorServiceTests()
    {
        _floorRepositoryMock = Substitute.For<IFloorRepository>();
        _buildingRepositoryMock = Substitute.For<IBuildingRepository>();
        _zoneRepositoryMock = Substitute.For<IZoneRepository>();
        _slotRepositoryMock = Substitute.For<IParkingSlotRepository>();
        _sessionRepositoryMock = Substitute.For<IRepository<PBMS.Domain.Entities.ParkingSession>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();
        _mapperMock = Substitute.For<IMapper>();

        _floorService = new FloorService(
            _floorRepositoryMock,
            _buildingRepositoryMock,
            _zoneRepositoryMock,
            _slotRepositoryMock,
            _mapperMock,
            _sessionRepositoryMock,
            _vehicleTypeRepositoryMock);
    }

    [Fact]
    public async Task CreateFloorAsync_ShouldReturnFloorDto_WhenRequestIsValid()
    {
        // Arrange
        var request = new FloorCreateRequest
        {
            BuildingId = 1,
            FloorNumber = 1,
            Name = "Floor 1"
        };

        var building = new Building { Id = 1 };
        var floorDto = new FloorDto { Id = 1, FloorNumber = 1, Name = "Floor 1", BuildingId = 1 };

        _buildingRepositoryMock.GetByIdAsync(request.BuildingId).Returns(building);
        _floorRepositoryMock.FloorNumberExistsInBuildingAsync(request.FloorNumber, request.BuildingId).Returns(false);
        _mapperMock.Map<FloorDto>(Arg.Any<Floor>()).Returns(floorDto);

        // Act
        var result = await _floorService.CreateFloorAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.FloorNumber, result.FloorNumber);
        await _floorRepositoryMock.Received(1).AddAsync(Arg.Any<Floor>());
    }

    [Fact]
    public async Task CreateFloorAsync_ShouldThrowNotFoundException_WhenBuildingDoesNotExist()
    {
        // Arrange
        var request = new FloorCreateRequest { BuildingId = 99 };
        _buildingRepositoryMock.GetByIdAsync(request.BuildingId).Returns((Building)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _floorService.CreateFloorAsync(request));
    }

    [Fact]
    public async Task CreateFloorAsync_ShouldThrowValidationException_WhenFloorNumberExists()
    {
        // Arrange
        var request = new FloorCreateRequest { BuildingId = 1, FloorNumber = 1 };
        var building = new Building { Id = 1 };
        _buildingRepositoryMock.GetByIdAsync(request.BuildingId).Returns(building);
        _floorRepositoryMock.FloorNumberExistsInBuildingAsync(request.FloorNumber, request.BuildingId).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _floorService.CreateFloorAsync(request));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task GetFloorByIdAsync_ShouldReturnFloorDto_WhenFloorExists()
    {
        // Arrange
        int floorId = 1;
        var floor = new Floor { Id = floorId, FloorNumber = 1 };
        var floorDto = new FloorDto { Id = floorId, FloorNumber = 1 };

        _floorRepositoryMock.GetByIdAsync(floorId).Returns(floor);
        _mapperMock.Map<FloorDto>(floor).Returns(floorDto);

        // Act
        var result = await _floorService.GetFloorByIdAsync(floorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(floorId, result.Id);
    }

    [Fact]
    public async Task UpdateFloorAsync_ShouldUpdateAndReturnDto_WhenRequestIsValid()
    {
        // Arrange
        int floorId = 1;
        var request = new FloorUpdateRequest { FloorNumber = 2, Name = "New Name", Status = FloorStatus.Active };
        var existingFloor = new Floor { Id = floorId, FloorNumber = 1, BuildingId = 1 };
        var updatedDto = new FloorDto { Id = floorId, FloorNumber = 2, Name = "New Name" };

        _floorRepositoryMock.GetByIdAsync(floorId).Returns(existingFloor);
        _floorRepositoryMock.FloorNumberExistsInBuildingAsync(request.FloorNumber, existingFloor.BuildingId).Returns(false);
        _mapperMock.Map<FloorDto>(existingFloor).Returns(updatedDto);

        // Act
        var result = await _floorService.UpdateFloorAsync(floorId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.FloorNumber, result.FloorNumber);
        _floorRepositoryMock.Received(1).Update(existingFloor);
    }

    [Fact]
    public async Task DeleteFloorAsync_ShouldRemoveFloor_WhenFloorHasNoZones()
    {
        // Arrange
        int floorId = 1;
        var floor = new Floor { Id = floorId, Zones = new List<Zone>() };
        _floorRepositoryMock.GetFloorWithDetailsAsync(floorId).Returns(floor);

        // Act
        await _floorService.DeleteFloorAsync(floorId);

        // Assert
        await _floorRepositoryMock.Received(1).RemoveAsync(floor);
    }

    [Fact]
    public async Task DeleteFloorAsync_ShouldThrowValidationException_WhenFloorHasZones()
    {
        // Arrange
        int floorId = 1;
        var floor = new Floor 
        { 
            Id = floorId, 
            Zones = new List<Zone> { new Zone { Id = 1 } } 
        };
        _floorRepositoryMock.GetFloorWithDetailsAsync(floorId).Returns(floor);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _floorService.DeleteFloorAsync(floorId));
        Assert.Contains("contains zones", exception.Message);
        await _floorRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Floor>());
    }

    [Fact]
    public async Task GetFloorsSlotSummaryAsync_ShouldThrowNotFoundException_WhenBuildingDoesNotExist()
    {
        // Arrange
        int buildingId = 99;
        _buildingRepositoryMock.GetByIdAsync(buildingId).Returns((Building)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _floorService.GetFloorsSlotSummaryAsync(buildingId, null, null));
    }

    [Fact]
    public async Task GetFloorsSlotSummaryAsync_ShouldReturnSummaries_WhenBuildingExists()
    {
        // Arrange
        int buildingId = 1;
        var building = new Building { Id = buildingId };
        var floors = new List<Floor> { new Floor { Id = 1, FloorNumber = 1, BuildingId = buildingId } };
        var zones = new List<Zone> 
        { 
            new Zone 
            { 
                Id = 1, 
                FloorId = 1, 
                VehicleTypeId = 1, 
                Capacity = 10,
                VehicleType = new VehicleType { Id = 1, TypeName = "Car" } 
            } 
        };
        var slots = new List<ParkingSlot> 
        { 
            new ParkingSlot { Id = 1, ZoneId = 1, VehicleTypeId = 1, Status = SlotStatus.Available } 
        };
        var sessions = new List<PBMS.Domain.Entities.ParkingSession>();

        _buildingRepositoryMock.GetByIdAsync(buildingId).Returns(building);
        _floorRepositoryMock.GetFloorsByBuildingIdAsync(buildingId).Returns(floors);
        _zoneRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Zone, bool>>>()).Returns(zones);
        _slotRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSlot, bool>>>()).Returns(slots);
        _sessionRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PBMS.Domain.Entities.ParkingSession, bool>>>()).Returns(sessions);

        // Act
        var result = await _floorService.GetFloorsSlotSummaryAsync(buildingId, null, null);

        // Assert
        Assert.NotNull(result);
        var summaryList = Assert.IsAssignableFrom<IEnumerable<FloorSlotSummaryDto>>(result);
        Assert.NotEmpty(summaryList);
        var firstSummary = summaryList.First();
        Assert.Equal(1, firstSummary.FloorId);
        Assert.Equal(1, firstSummary.FloorNumber);
        Assert.Equal(1, firstSummary.TotalSlots);
        Assert.Single(firstSummary.VehicleTypeSummaries);
        Assert.Equal("Car", firstSummary.VehicleTypeSummaries[0].VehicleTypeName);
        Assert.Equal(1, firstSummary.VehicleTypeSummaries[0].StatusCounts.First(sc => sc.Status == "Available").Count);
    }
}

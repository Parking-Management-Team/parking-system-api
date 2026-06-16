using AutoMapper;
using NSubstitute;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Application.ParkingStructure.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using Xunit;

namespace PBMS.UnitTests.ParkingStructure;

public class ZoneServiceTests
{
    private readonly IZoneRepository _zoneRepositoryMock;
    private readonly IRepository<Floor> _floorRepositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly ZoneService _zoneService;

    public ZoneServiceTests()
    {
        _zoneRepositoryMock = Substitute.For<IZoneRepository>();
        _floorRepositoryMock = Substitute.For<IRepository<Floor>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();
        _mapperMock = Substitute.For<IMapper>();

        _zoneService = new ZoneService(
            _zoneRepositoryMock,
            _floorRepositoryMock,
            _vehicleTypeRepositoryMock,
            _mapperMock);
    }

    [Fact]
    public async Task CreateZoneAsync_ShouldReturnZoneDto_WhenRequestIsValid()
    {
        // Arrange
        var request = new ZoneCreateRequest
        {
            FloorId = 1,
            Code = "ZA",
            Name = "Zone A",
            VehicleTypeId = 1,
            Capacity = 50
        };

        var floor = new Floor { Id = 1 };
        var vehicleType = new VehicleType { Id = 1 };
        var zone = new Zone { Id = 1, Name = "Zone A", FloorId = 1 };
        var zoneDto = new ZoneDto { Id = 1, Name = "Zone A", FloorId = 1 };

        _floorRepositoryMock.GetByIdAsync(request.FloorId).Returns(floor);
        _zoneRepositoryMock.ZoneNameExistsInFloorAsync(request.Name, request.FloorId).Returns(false);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _mapperMock.Map<ZoneDto>(Arg.Any<Zone>()).Returns(zoneDto);

        // Act
        var result = await _zoneService.CreateZoneAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        await _zoneRepositoryMock.Received(1).AddAsync(Arg.Any<Zone>());
    }

    [Fact]
    public async Task CreateZoneAsync_ShouldThrowNotFoundException_WhenFloorDoesNotExist()
    {
        // Arrange
        var request = new ZoneCreateRequest { FloorId = 99 };
        _floorRepositoryMock.GetByIdAsync(request.FloorId).Returns((Floor)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _zoneService.CreateZoneAsync(request));
    }

    [Fact]
    public async Task CreateZoneAsync_ShouldThrowValidationException_WhenNameAlreadyExists()
    {
        // Arrange
        var request = new ZoneCreateRequest { FloorId = 1, Name = "Existing Zone" };
        var floor = new Floor { Id = 1 };
        _floorRepositoryMock.GetByIdAsync(request.FloorId).Returns(floor);
        _zoneRepositoryMock.ZoneNameExistsInFloorAsync(request.Name, request.FloorId).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _zoneService.CreateZoneAsync(request));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task GetZoneByIdAsync_ShouldReturnZoneDto_WhenZoneExists()
    {
        // Arrange
        int zoneId = 1;
        var zone = new Zone { Id = zoneId, Name = "Zone A" };
        var zoneDto = new ZoneDto { Id = zoneId, Name = "Zone A" };

        _zoneRepositoryMock.GetZoneWithDetailsAsync(zoneId).Returns(zone);
        _mapperMock.Map<ZoneDto>(zone).Returns(zoneDto);

        // Act
        var result = await _zoneService.GetZoneByIdAsync(zoneId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(zoneId, result.Id);
    }

    [Fact]
    public async Task GetZoneByIdAsync_ShouldThrowNotFoundException_WhenZoneDoesNotExist()
    {
        // Arrange
        int zoneId = 1;
        _zoneRepositoryMock.GetZoneWithDetailsAsync(zoneId).Returns((Zone)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _zoneService.GetZoneByIdAsync(zoneId));
    }

    [Fact]
    public async Task UpdateZoneAsync_ShouldUpdateAndReturnDto_WhenRequestIsValid()
    {
        // Arrange
        int zoneId = 1;
        var request = new ZoneUpdateRequest { Code = "ZB", Name = "New Name", VehicleTypeId = 2, Capacity = 100 };
        var existingZone = new Zone { Id = zoneId, Name = "Old Name", FloorId = 1 };
        var vehicleType = new VehicleType { Id = 2 };
        var updatedDto = new ZoneDto { Id = zoneId, Name = "New Name" };

        _zoneRepositoryMock.GetByIdAsync(zoneId).Returns(existingZone);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _zoneRepositoryMock.ZoneNameExistsInFloorAsync(request.Name, existingZone.FloorId).Returns(false);
        _mapperMock.Map<ZoneDto>(existingZone).Returns(updatedDto);

        // Act
        var result = await _zoneService.UpdateZoneAsync(zoneId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        _zoneRepositoryMock.Received(1).Update(existingZone);
    }

    [Fact]
    public async Task DeleteZoneAsync_ShouldRemoveZone_WhenZoneHasNoSlots()
    {
        // Arrange
        int zoneId = 1;
        var zone = new Zone { Id = zoneId, ParkingSlots = new List<ParkingSlot>() };
        _zoneRepositoryMock.GetZoneWithDetailsAsync(zoneId).Returns(zone);

        // Act
        await _zoneService.DeleteZoneAsync(zoneId);

        // Assert
        await _zoneRepositoryMock.Received(1).RemoveAsync(zone);
    }

    [Fact]
    public async Task DeleteZoneAsync_ShouldThrowValidationException_WhenZoneHasSlots()
    {
        // Arrange
        int zoneId = 1;
        var zone = new Zone 
        { 
            Id = zoneId, 
            ParkingSlots = new List<ParkingSlot> { new ParkingSlot { Id = 1 } } 
        };
        _zoneRepositoryMock.GetZoneWithDetailsAsync(zoneId).Returns(zone);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _zoneService.DeleteZoneAsync(zoneId));
        Assert.Contains("contains parking slots", exception.Message);
        await _zoneRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Zone>());
    }
}

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

public class ParkingSlotServiceTests
{
    private readonly IParkingSlotRepository _slotRepositoryMock;
    private readonly IRepository<Zone> _zoneRepositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepositoryMock;
    private readonly IRepository<Booking> _bookingRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly ParkingSlotService _slotService;

    public ParkingSlotServiceTests()
    {
        _slotRepositoryMock = Substitute.For<IParkingSlotRepository>();
        _zoneRepositoryMock = Substitute.For<IRepository<Zone>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleType>>();
        _bookingRepositoryMock = Substitute.For<IRepository<Booking>>();
        _mapperMock = Substitute.For<IMapper>();

        _slotService = new ParkingSlotService(
            _slotRepositoryMock,
            _zoneRepositoryMock,
            _vehicleTypeRepositoryMock,
            _bookingRepositoryMock,
            _mapperMock);
    }

    [Fact]
    public async Task CreateSlotAsync_ShouldReturnSlotDto_WhenRequestIsValid()
    {
        // Arrange
        var request = new ParkingSlotCreateRequest
        {
            ZoneId = 1,
            VehicleTypeId = 1,
            Code = "SLOT-01",
            Name = "Slot 1"
        };

        var zone = new Zone { Id = 1, VehicleTypeId = 1, Capacity = 10 };
        var vehicleType = new VehicleType { Id = 1, TypeName = "Car" };
        var slotDto = new ParkingSlotDto { Id = 1, Code = "SLOT-01", ZoneId = 1 };

        _zoneRepositoryMock.GetByIdAsync(request.ZoneId).Returns(zone);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _slotRepositoryMock.SlotCodeExistsAsync(request.Code).Returns(false);
        _slotRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSlot, bool>>>()).Returns(new List<ParkingSlot>());
        _mapperMock.Map<ParkingSlotDto>(Arg.Any<ParkingSlot>()).Returns(slotDto);

        // Act
        var result = await _slotService.CreateSlotAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Code, result.Code);
        await _slotRepositoryMock.Received(1).AddAsync(Arg.Any<ParkingSlot>());
    }

    [Fact]
    public async Task CreateSlotAsync_ShouldThrowNotFoundException_WhenZoneDoesNotExist()
    {
        // Arrange
        var request = new ParkingSlotCreateRequest { ZoneId = 99 };
        _zoneRepositoryMock.GetByIdAsync(request.ZoneId).Returns((Zone)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _slotService.CreateSlotAsync(request));
    }

    [Fact]
    public async Task CreateSlotAsync_ShouldThrowValidationException_WhenSlotCodeExists()
    {
        // Arrange
        var request = new ParkingSlotCreateRequest { ZoneId = 1, VehicleTypeId = 1, Code = "EXISTING" };
        var zone = new Zone { Id = 1, VehicleTypeId = 1, Capacity = 10 };
        var vehicleType = new VehicleType { Id = 1, TypeName = "Car" };

        _zoneRepositoryMock.GetByIdAsync(request.ZoneId).Returns(zone);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _slotRepositoryMock.SlotCodeExistsAsync(request.Code).Returns(true);
        _slotRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSlot, bool>>>()).Returns(new List<ParkingSlot>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _slotService.CreateSlotAsync(request));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task CreateSlotAsync_ShouldThrowValidationException_WhenZoneCapacityExceeded()
    {
        // Arrange
        var request = new ParkingSlotCreateRequest { ZoneId = 1, VehicleTypeId = 1, Code = "NEW-SLOT" };
        var zone = new Zone { Id = 1, VehicleTypeId = 1, Capacity = 2 };
        var vehicleType = new VehicleType { Id = 1, TypeName = "Car" };
        
        // Mock existing slots meeting the capacity limit
        var existingSlots = new List<ParkingSlot> 
        { 
            new ParkingSlot { Id = 1, Code = "SLOT-01" }, 
            new ParkingSlot { Id = 2, Code = "SLOT-02" } 
        };

        _zoneRepositoryMock.GetByIdAsync(request.ZoneId).Returns(zone);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _slotRepositoryMock.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSlot, bool>>>()).Returns(existingSlots);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _slotService.CreateSlotAsync(request));
        Assert.Contains("reached its maximum capacity", exception.Message);
    }

    [Fact]
    public async Task UpdateSlotAsync_ShouldUpdateAndReturnDto_WhenRequestIsValid()
    {
        // Arrange
        int slotId = 1;
        var request = new ParkingSlotUpdateRequest { Code = "NEW-CODE", VehicleTypeId = 2, Status = SlotStatus.Blocked };
        var existingSlot = new ParkingSlot { Id = slotId, Code = "OLD-CODE", ZoneId = 1 };
        var vehicleType = new VehicleType { Id = 2 };
        var zone = new Zone { Id = 1, VehicleTypeId = 2 };
        var updatedDto = new ParkingSlotDto { Id = slotId, Code = "NEW-CODE" };

        _slotRepositoryMock.GetByIdAsync(slotId).Returns(existingSlot);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _zoneRepositoryMock.GetByIdAsync(existingSlot.ZoneId).Returns(zone);
        _slotRepositoryMock.SlotCodeExistsAsync(request.Code).Returns(false);
        _mapperMock.Map<ParkingSlotDto>(existingSlot).Returns(updatedDto);

        // Act
        var result = await _slotService.UpdateSlotAsync(slotId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Code, result.Code);
        _slotRepositoryMock.Received(1).Update(existingSlot);
    }

    [Fact]
    public async Task UpdateSlotAsync_ShouldThrowValidationException_WhenVehicleTypeIdDoesNotMatchZoneVehicleTypeId()
    {
        // Arrange
        int slotId = 1;
        var request = new ParkingSlotUpdateRequest { Code = "NEW-CODE", VehicleTypeId = 2, Status = SlotStatus.Blocked };
        var existingSlot = new ParkingSlot { Id = slotId, Code = "OLD-CODE", ZoneId = 1 };
        var vehicleType = new VehicleType { Id = 2 };
        var zone = new Zone { Id = 1, VehicleTypeId = 1 }; // Zone vehicle type is 1, request vehicle type is 2

        _slotRepositoryMock.GetByIdAsync(slotId).Returns(existingSlot);
        _vehicleTypeRepositoryMock.GetByIdAsync(request.VehicleTypeId).Returns(vehicleType);
        _zoneRepositoryMock.GetByIdAsync(existingSlot.ZoneId).Returns(zone);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _slotService.UpdateSlotAsync(slotId, request));
        Assert.Contains("does not match the Zone's VehicleTypeId", exception.Message);
    }

    [Fact]
    public async Task DeleteSlotAsync_ShouldRemoveSlot_WhenStatusIsAvailable()
    {
        // Arrange
        int slotId = 1;
        var slot = new ParkingSlot { Id = slotId, Code = "A1", Status = SlotStatus.Available, ParkingSessions = new List<ParkingSession>() };
        _slotRepositoryMock.GetSlotWithDetailsAsync(slotId).Returns(slot);

        // Act
        await _slotService.DeleteSlotAsync(slotId);

        // Assert
        await _slotRepositoryMock.Received(1).RemoveAsync(slot);
    }

    [Fact]
    public async Task DeleteSlotAsync_ShouldThrowValidationException_WhenStatusIsOccupied()
    {
        // Arrange
        int slotId = 1;
        var slot = new ParkingSlot { Id = slotId, Code = "A1", Status = SlotStatus.Occupied };
        _slotRepositoryMock.GetSlotWithDetailsAsync(slotId).Returns(slot);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _slotService.DeleteSlotAsync(slotId));
        Assert.Contains("status is Occupied", exception.Message);
    }

    [Fact]
    public async Task DeleteSlotAsync_ShouldThrowValidationException_WhenHasActiveSession()
    {
        // Arrange
        int slotId = 1;
        var slot = new ParkingSlot 
        { 
            Id = slotId, 
            Code = "A1", 
            Status = SlotStatus.Available,
            ParkingSessions = new List<ParkingSession> { new ParkingSession { SessionStatus = "Active" } } 
        };
        _slotRepositoryMock.GetSlotWithDetailsAsync(slotId).Returns(slot);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _slotService.DeleteSlotAsync(slotId));
        Assert.Contains("active parking session", exception.Message);
    }
}

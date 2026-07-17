using NSubstitute;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;
using PBMS.Domain.Entities;

namespace PBMS.UnitTests;

public class VehicleServiceTests
{
    private readonly IVehicleRepository _vehicleRepositoryMock;
    private readonly IVehicleTypeRepository _vehicleTypeRepositoryMock;
    private readonly VehicleService _vehicleService;

    public VehicleServiceTests()
    {
        _vehicleRepositoryMock = Substitute.For<IVehicleRepository>();
        _vehicleTypeRepositoryMock = Substitute.For<IVehicleTypeRepository>();
        _vehicleService = new VehicleService(_vehicleRepositoryMock, _vehicleTypeRepositoryMock);
    }

    [Theory]
    [InlineData("51A-123.45", "51A12345")]
    [InlineData("51a 12345", "51A12345")]
    [InlineData(" 51A12345 ", "51A12345")]
    public void NormalizeLicensePlate_RemovesSeparatorsAndUppercases(string input, string expected)
    {
        var normalized = VehicleService.NormalizeLicensePlate(input);

        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("51A-123.45", "Car")]
    [InlineData("30F-5678", "Car")]
    [InlineData("29G1-123.45", "Motorcycle")]
    [InlineData("29-G1 123.45", "Motorcycle")]
    [InlineData("59T2-888.88", "Motorcycle")]
    [InlineData("29AA-123.45", "Motorcycle")]
    [InlineData("51LD-123.45", "Car")]
    [InlineData("80NG-123.45", "Car")]
    [InlineData("AA-12-34", "Car")]
    [InlineData("59MD-12345", "Motorcycle")]
    public void DetectVehicleTypeFromPlate_DetectsCorrectly(string input, string expected)
    {
        var result = VehicleService.DetectVehicleTypeFromPlate(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateAsync_CreatesVehicle_WhenInputIsValid()
    {
        var request = new CreateVehicleDto
        {
            AccountId = 10,
            VehicleTypeId = 1,
            LicensePlate = "51a-123.45"
        };
        var activeVehicleType = new VehicleType
        {
            Id = 1,
            TypeName = VehicleType.CarTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        };

        _vehicleRepositoryMock.AccountExistsAsync(10).Returns(true);
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(activeVehicleType);
        _vehicleRepositoryMock.GetByLicensePlateAsync("51A12345").Returns((Vehicle?)null);
        _vehicleRepositoryMock.AddAsync(Arg.Any<Vehicle>()).Returns(call =>
        {
            var vehicle = call.Arg<Vehicle>();
            vehicle.Id = 99;
            vehicle.VehicleType = activeVehicleType;
            return vehicle;
        });

        var result = await _vehicleService.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(99, result.Data!.Id);
        Assert.Equal("51A12345", result.Data.LicensePlate);
        Assert.Equal(Vehicle.StatusActive, result.Data.VehicleStatus);
        await _vehicleRepositoryMock.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.AccountId == 10
            && v.VehicleTypeId == 1
            && v.LicensePlate == "51A12345"
            && v.VehicleStatus == Vehicle.StatusActive));
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateLicensePlate()
    {
        var request = new CreateVehicleDto
        {
            VehicleTypeId = 1,
            LicensePlate = "51A-123.45"
        };
        var activeVehicleType = new VehicleType
        {
            Id = 1,
            TypeName = VehicleType.CarTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(activeVehicleType);
        _vehicleRepositoryMock.GetByLicensePlateAsync("51A12345").Returns(new Vehicle
        {
            Id = 99,
            LicensePlate = "51A12345",
            AccountId = 20, // Already belongs to another account
            VehicleTypeId = 1,
            VehicleType = activeVehicleType
        });

        var result = await _vehicleService.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Equal("LICENSE_PLATE_EXISTS", result.ErrorCode);
        await _vehicleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
    }

    [Fact]
    public async Task CreateAsync_ClaimsGuestVehicle_WhenLicensePlateExistsAsGuest()
    {
        var request = new CreateVehicleDto
        {
            AccountId = 10,
            VehicleTypeId = 1,
            LicensePlate = "51A-123.45",
            VehicleStatus = Vehicle.StatusActive
        };
        var activeVehicleType = new VehicleType
        {
            Id = 1,
            TypeName = VehicleType.CarTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        };

        _vehicleRepositoryMock.AccountExistsAsync(10).Returns(true);
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(activeVehicleType);
        
        var existingGuestVehicle = new Vehicle
        {
            Id = 99,
            LicensePlate = "51A12345",
            AccountId = null, // Guest vehicle
            VehicleTypeId = 2, // Old guest type
            VehicleType = activeVehicleType
        };
        _vehicleRepositoryMock.GetByLicensePlateAsync("51A12345").Returns(existingGuestVehicle);
        _vehicleRepositoryMock.UpdateAsync(Arg.Any<Vehicle>()).Returns(call =>
        {
            var vehicle = call.Arg<Vehicle>();
            return vehicle;
        });

        var result = await _vehicleService.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(99, result.Data!.Id);
        Assert.Equal(10, result.Data.AccountId);
        Assert.Equal(1, result.Data.VehicleTypeId);
        Assert.Equal("51A12345", result.Data.LicensePlate);
        await _vehicleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
        await _vehicleRepositoryMock.Received(1).UpdateAsync(Arg.Is<Vehicle>(v => v.Id == 99 && v.AccountId == 10));
    }

    [Fact]
    public async Task CreateAsync_RejectsInactiveVehicleType()
    {
        var request = new CreateVehicleDto
        {
            VehicleTypeId = 2,
            LicensePlate = "51A12345"
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(2).Returns(new VehicleType
        {
            Id = 2,
            TypeName = VehicleType.CarTypeName,
            VehicleTypeStatus = VehicleType.StatusInactive
        });

        var result = await _vehicleService.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Equal("VEHICLE_TYPE_INACTIVE", result.ErrorCode);
        await _vehicleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
    }

    [Fact]
    public async Task ArchiveAsync_RejectsVehicleWithActiveParkingSession()
    {
        var vehicle = new Vehicle
        {
            Id = 5,
            VehicleTypeId = 1,
            LicensePlate = "51A12345",
            VehicleStatus = Vehicle.StatusActive
        };
        _vehicleRepositoryMock.GetByIdAsync(5).Returns(vehicle);
        _vehicleRepositoryMock.HasActiveParkingSessionAsync(5).Returns(true);

        var result = await _vehicleService.ArchiveAsync(5);

        Assert.False(result.Success);
        Assert.Equal("VEHICLE_IN_ACTIVE_SESSION", result.ErrorCode);
        await _vehicleRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<Vehicle>());
    }
}

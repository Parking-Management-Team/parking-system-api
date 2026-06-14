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
            TypeName = VehicleType.MotorcycleTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        };

        _vehicleRepositoryMock.AccountExistsAsync(10).Returns(true);
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(activeVehicleType);
        _vehicleRepositoryMock.LicensePlateExistsAsync("51A12345").Returns(false);
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
        Assert.Equal("51a-123.45", result.Data.LicensePlate);
        Assert.Equal(Vehicle.StatusActive, result.Data.VehicleStatus);
        await _vehicleRepositoryMock.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.AccountId == 10
            && v.VehicleTypeId == 1
            && v.LicensePlate == "51a-123.45"
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
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(new VehicleType
        {
            Id = 1,
            TypeName = VehicleType.CarTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        });
        _vehicleRepositoryMock.LicensePlateExistsAsync("51A12345").Returns(true);

        var result = await _vehicleService.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Equal("LICENSE_PLATE_EXISTS", result.ErrorCode);
        await _vehicleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
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

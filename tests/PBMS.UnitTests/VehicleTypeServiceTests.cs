using NSubstitute;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;
using PBMS.Domain.Entities;

namespace PBMS.UnitTests;

public class VehicleTypeServiceTests
{
    private readonly IVehicleTypeRepository _vehicleTypeRepositoryMock;
    private readonly VehicleTypeService _vehicleTypeService;

    public VehicleTypeServiceTests()
    {
        _vehicleTypeRepositoryMock = Substitute.For<IVehicleTypeRepository>();
        _vehicleTypeService = new VehicleTypeService(_vehicleTypeRepositoryMock);
    }

    [Fact]
    public async Task CreateAsync_UsesTypeNameAndUppercaseStatus()
    {
        var request = new CreateVehicleTypeDto
        {
            Name = " motorcycle ",
            VehicleTypeStatus = "active"
        };
        _vehicleTypeRepositoryMock.NameExistsAsync("motorcycle").Returns(false);
        _vehicleTypeRepositoryMock.AddAsync(Arg.Any<VehicleType>()).Returns(call =>
        {
            var vehicleType = call.Arg<VehicleType>();
            vehicleType.Id = 1;
            return vehicleType;
        });

        var result = await _vehicleTypeService.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal("motorcycle", result.Data!.Name);
        Assert.Equal(VehicleType.StatusActive, result.Data.VehicleTypeStatus);
        await _vehicleTypeRepositoryMock.Received(1).AddAsync(Arg.Is<VehicleType>(vt =>
            vt.TypeName == "motorcycle"
            && vt.VehicleTypeStatus == VehicleType.StatusActive));
    }

    [Fact]
    public async Task DeleteAsync_ArchivesVehicleType_WhenItIsNotInUse()
    {
        var vehicleType = new VehicleType
        {
            Id = 1,
            TypeName = VehicleType.MotorcycleTypeName,
            VehicleTypeStatus = VehicleType.StatusActive
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _vehicleTypeRepositoryMock.IsUsedInSessionsAsync(1).Returns(false);
        _vehicleTypeRepositoryMock.IsUsedInBookingsAsync(1).Returns(false);
        _vehicleTypeRepositoryMock.UpdateAsync(Arg.Any<VehicleType>()).Returns(call => call.Arg<VehicleType>());

        var result = await _vehicleTypeService.DeleteAsync(1);

        Assert.True(result.Success);
        await _vehicleTypeRepositoryMock.Received(1).UpdateAsync(Arg.Is<VehicleType>(vt =>
            vt.Id == 1 && vt.VehicleTypeStatus == VehicleType.StatusInactive));
        await _vehicleTypeRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }
}

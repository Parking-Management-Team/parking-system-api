using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;
using PBMS.Domain.Entities;

namespace PBMS.UnitTests;

public class VehicleTypeServiceTests
{
    private readonly IVehicleTypeRepository _vehicleTypeRepositoryMock;
    private readonly IPricingPolicyRepository _pricingPolicyRepositoryMock;
    private readonly VehicleTypeService _vehicleTypeService;

    public VehicleTypeServiceTests()
    {
        _vehicleTypeRepositoryMock = Substitute.For<IVehicleTypeRepository>();
        _pricingPolicyRepositoryMock = Substitute.For<IPricingPolicyRepository>();
        _vehicleTypeService = new VehicleTypeService(_vehicleTypeRepositoryMock, _pricingPolicyRepositoryMock);
    }

    [Fact]
    public async Task CreateAsync_UsesTypeNameAndUppercaseStatus_WhenCreatedAsInactive()
    {
        var request = new CreateVehicleTypeDto
        {
            Name = " motorcycle ",
            VehicleTypeStatus = "inactive"
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
        Assert.Equal(VehicleType.StatusInactive, result.Data.VehicleTypeStatus);
        await _vehicleTypeRepositoryMock.Received(1).AddAsync(Arg.Is<VehicleType>(vt =>
            vt.TypeName == "motorcycle"
            && vt.VehicleTypeStatus == VehicleType.StatusInactive));
    }

    [Fact]
    public async Task CreateAsync_WhenCreatedAsActive_ShouldFail_PricingPolicyRequired()
    {
        var request = new CreateVehicleTypeDto
        {
            Name = " motorcycle ",
            VehicleTypeStatus = "active"
        };

        var result = await _vehicleTypeService.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Equal("PRICING_POLICY_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_ActivateWithPricingPolicy_ShouldSucceed()
    {
        var vehicleType = new VehicleType
        {
            Id = 1,
            TypeName = "Motorcycle",
            VehicleTypeStatus = VehicleType.StatusInactive
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _vehicleTypeRepositoryMock.NameExistsAsync("Motorcycle", 1).Returns(false);
        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(new PricingPolicy());
        _vehicleTypeRepositoryMock.UpdateAsync(Arg.Any<VehicleType>()).Returns(call => call.Arg<VehicleType>());

        var request = new UpdateVehicleTypeDto
        {
            Name = "Motorcycle",
            VehicleTypeStatus = "active"
        };

        var result = await _vehicleTypeService.UpdateAsync(1, request);

        Assert.True(result.Success);
        Assert.Equal(VehicleType.StatusActive, result.Data!.VehicleTypeStatus);
    }

    [Fact]
    public async Task UpdateAsync_ActivateWithoutPricingPolicy_ShouldFail()
    {
        var vehicleType = new VehicleType
        {
            Id = 1,
            TypeName = "Motorcycle",
            VehicleTypeStatus = VehicleType.StatusInactive
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _vehicleTypeRepositoryMock.NameExistsAsync("Motorcycle", 1).Returns(false);
        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns((PricingPolicy?)null);

        var request = new UpdateVehicleTypeDto
        {
            Name = "Motorcycle",
            VehicleTypeStatus = "active"
        };

        var result = await _vehicleTypeService.UpdateAsync(1, request);

        Assert.False(result.Success);
        Assert.Equal("PRICING_POLICY_REQUIRED", result.ErrorCode);
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

    [Theory]
    [InlineData(-5)]
    [InlineData(105)]
    public async Task CreateAsync_ShouldFail_WhenBufferRatioIsInvalid(int invalidBufferRatio)
    {
        // Arrange
        var request = new CreateVehicleTypeDto
        {
            Name = "motorcycle",
            VehicleTypeStatus = "inactive",
            BufferRatio = invalidBufferRatio
        };
        _vehicleTypeRepositoryMock.NameExistsAsync("motorcycle").Returns(false);

        // Act
        var result = await _vehicleTypeService.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("INVALID_BUFFER_RATIO", result.ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task UpdateAsync_ShouldFail_WhenBufferRatioIsInvalid(int invalidBufferRatio)
    {
        // Arrange
        var vehicleType = new VehicleType
        {
            Id = 1,
            TypeName = "Motorcycle",
            VehicleTypeStatus = VehicleType.StatusInactive
        };
        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _vehicleTypeRepositoryMock.NameExistsAsync("Motorcycle", 1).Returns(false);

        var request = new UpdateVehicleTypeDto
        {
            Name = "Motorcycle",
            BufferRatio = invalidBufferRatio,
            VehicleTypeStatus = "inactive"
        };

        // Act
        var result = await _vehicleTypeService.UpdateAsync(1, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("INVALID_BUFFER_RATIO", result.ErrorCode);
    }
}

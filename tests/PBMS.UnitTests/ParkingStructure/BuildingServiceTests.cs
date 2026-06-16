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

public class BuildingServiceTests
{
    private readonly IBuildingRepository _buildingRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly BuildingService _buildingService;

    public BuildingServiceTests()
    {
        _buildingRepositoryMock = Substitute.For<IBuildingRepository>();
        _mapperMock = Substitute.For<IMapper>();

        _buildingService = new BuildingService(_buildingRepositoryMock, _mapperMock);
    }

    [Fact]
    public async Task CreateBuildingAsync_ShouldReturnBuildingDto_WhenRequestIsValid()
    {
        // Arrange
        var request = new BuildingCreateRequest
        {
            Name = "Building 1",
            Address = "123 Street",
            TotalFloor = 5
        };

        var buildingDto = new BuildingDto { Id = 1, Name = "Building 1" };

        _mapperMock.Map<BuildingDto>(Arg.Any<Building>()).Returns(buildingDto);

        // Act
        var result = await _buildingService.CreateBuildingAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        await _buildingRepositoryMock.Received(1).AddAsync(Arg.Any<Building>());
    }

    [Fact]
    public async Task GetBuildingByIdAsync_ShouldReturnBuildingDto_WhenExists()
    {
        // Arrange
        int id = 1;
        var building = new Building { Id = id, Name = "Building 1" };
        var buildingDto = new BuildingDto { Id = id, Name = "Building 1" };

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
        var request = new BuildingUpdateRequest { Name = "New Name", Status = BuildingStatus.Available };
        var existingBuilding = new Building { Id = id, Name = "Old Name" };
        var updatedDto = new BuildingDto { Id = id, Name = "New Name" };

        _buildingRepositoryMock.GetByIdAsync(id).Returns(existingBuilding);
        _mapperMock.Map<BuildingDto>(existingBuilding).Returns(updatedDto);

        // Act
        var result = await _buildingService.UpdateBuildingAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        _buildingRepositoryMock.Received(1).Update(existingBuilding);
    }

    [Fact]
    public async Task DeleteBuildingAsync_ShouldRemove_WhenNoFloorsExist()
    {
        // Arrange
        int id = 1;
        var building = new Building { Id = id, Name = "Building 1", Floors = new List<Floor>() };
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
            Name = "Building 1", 
            Floors = new List<Floor> { new Floor { Id = 1 } } 
        };
        _buildingRepositoryMock.GetBuildingWithDetailsAsync(id).Returns(building);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _buildingService.DeleteBuildingAsync(id));
        Assert.Contains("contains floors", exception.Message);
        await _buildingRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Building>());
    }
}

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
        var request = new BuildingUpdateRequest { Code = "NEW-CODE", Name = "New Name", Status = BuildingStatus.Available };
        var existingBuilding = new Building { Id = id, Code = "OLD-CODE" };
        var updatedDto = new BuildingDto { Id = id, Code = "NEW-CODE" };

        _buildingRepositoryMock.GetByIdAsync(id).Returns(existingBuilding);
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
}

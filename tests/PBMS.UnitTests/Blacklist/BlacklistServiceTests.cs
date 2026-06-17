using AutoMapper;
using NSubstitute;
using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Blacklist.Services;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using Xunit;

namespace PBMS.UnitTests.Blacklist;

public class BlacklistServiceTests
{
    private readonly IBlacklistRepository _blacklistRepositoryMock;
    private readonly IRepository<Vehicle> _vehicleRepositoryMock;
    private readonly ICardRepository _cardRepositoryMock;
    private readonly IRepository<Incident> _incidentRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly BlacklistService _blacklistService;

    public BlacklistServiceTests()
    {
        _blacklistRepositoryMock = Substitute.For<IBlacklistRepository>();
        _vehicleRepositoryMock = Substitute.For<IRepository<Vehicle>>();
        _cardRepositoryMock = Substitute.For<ICardRepository>();
        _incidentRepositoryMock = Substitute.For<IRepository<Incident>>();
        _mapperMock = Substitute.For<IMapper>();

        _blacklistService = new BlacklistService(
            _blacklistRepositoryMock,
            _vehicleRepositoryMock,
            _cardRepositoryMock,
            _incidentRepositoryMock,
            _mapperMock);
    }

    [Fact]
    public async Task AddToBlacklistAsync_ShouldCallAddAsync_WhenRequestIsValid()
    {
        // Arrange
        var request = new AddToBlacklistRequest { VehicleId = 1, Reason = "Test" };
        var vehicle = new Vehicle { Id = 1 };
        _vehicleRepositoryMock.GetByIdAsync(1).Returns(vehicle);

        // Act
        await _blacklistService.AddToBlacklistAsync(request);

        // Assert
        await _blacklistRepositoryMock.Received(1).AddAsync(Arg.Any<PBMS.Domain.Entities.Blacklist>());
        await _blacklistRepositoryMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AddToBlacklistAsync_ShouldThrowNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        var request = new AddToBlacklistRequest { VehicleId = 999, Reason = "Test" };
        _vehicleRepositoryMock.GetByIdAsync(999).Returns((Vehicle)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _blacklistService.AddToBlacklistAsync(request));
    }

    [Fact]
    public async Task RemoveFromBlacklistAsync_ShouldCallRemove_WhenExists()
    {
        // Arrange
        var blacklist = new PBMS.Domain.Entities.Blacklist { Id = 1 };
        _blacklistRepositoryMock.GetByIdAsync(1).Returns(blacklist);

        // Act
        await _blacklistService.RemoveFromBlacklistAsync(1);

        // Assert
        await _blacklistRepositoryMock.Received(1).RemoveAsync(blacklist);
        await _blacklistRepositoryMock.Received(1).SaveChangesAsync();
    }
}

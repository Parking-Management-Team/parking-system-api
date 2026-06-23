using AutoMapper;
using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Services;
using PBMS.Domain.Entities;
using PBMS.Application.Common.Exceptions;

namespace PBMS.UnitTests;

public class SubscriptionPriceConfigServiceTests
{
    private readonly ISubscriptionPriceConfigRepository _repositoryMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepoMock;
    private readonly IMapper _mapperMock;
    private readonly SubscriptionPriceConfigService _service;

    public SubscriptionPriceConfigServiceTests()
    {
        _repositoryMock = Substitute.For<ISubscriptionPriceConfigRepository>();
        _vehicleTypeRepoMock = Substitute.For<IRepository<VehicleType>>();
        _mapperMock = Substitute.For<IMapper>();
        _service = new SubscriptionPriceConfigService(_repositoryMock, _vehicleTypeRepoMock, _mapperMock);
    }

    private static VehicleType MakeVehicleType(int id = 1, string name = "Xe máy") =>
        new VehicleType { Id = id, TypeName = name };

    private static SubscriptionPriceConfig MakeConfig(int id = 1, int vehicleTypeId = 1, decimal price = 120000m, bool isActive = true) =>
        new SubscriptionPriceConfig
        {
            Id = id,
            VehicleTypeId = vehicleTypeId,
            Price = price,
            EffectiveFrom = DateTime.UtcNow.AddDays(-30),
            EffectiveTo = null,
            IsActive = isActive,
            VehicleType = MakeVehicleType(vehicleTypeId)
        };

    private static SubscriptionPriceConfigDto MakeDto(int id = 1, int vehicleTypeId = 1, decimal price = 120000m, bool isActive = true) =>
        new SubscriptionPriceConfigDto
        {
            Id = id,
            VehicleTypeId = vehicleTypeId,
            VehicleTypeName = "Xe máy",
            Price = price,
            EffectiveFrom = DateTime.UtcNow.AddDays(-30),
            EffectiveTo = null,
            IsActive = isActive
        };

    // =========================================================================
    // GetActiveConfigByVehicleTypeAsync
    // =========================================================================

    [Fact]
    public async Task GetActiveConfigByVehicleTypeAsync_ShouldReturnConfig_WhenExists()
    {
        var vehicleType = MakeVehicleType();
        var config = MakeConfig();

        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(vehicleType);
        _repositoryMock.GetActiveConfigByVehicleTypeAsync(1).Returns(config);
        _mapperMock.Map<SubscriptionPriceConfigDto>(config).Returns(MakeDto());

        var result = await _service.GetActiveConfigByVehicleTypeAsync(1);

        Assert.NotNull(result);
        Assert.Equal(120000m, result!.Price);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetActiveConfigByVehicleTypeAsync_ShouldReturnNull_WhenNoActiveConfig()
    {
        var vehicleType = MakeVehicleType();

        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(vehicleType);
        _repositoryMock.GetActiveConfigByVehicleTypeAsync(1).Returns((SubscriptionPriceConfig?)null);

        var result = await _service.GetActiveConfigByVehicleTypeAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveConfigByVehicleTypeAsync_ShouldThrow_WhenVehicleTypeNotFound()
    {
        _vehicleTypeRepoMock.GetByIdAsync(999).Returns((VehicleType?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetActiveConfigByVehicleTypeAsync(999));
        Assert.Equal("NOT_FOUND", ex.ErrorCode);
    }

    // =========================================================================
    // CreateConfigAsync
    // =========================================================================

    [Fact]
    public async Task CreateConfigAsync_ShouldCreate_WhenNoExistingActiveConfig()
    {
        var vehicleType = MakeVehicleType();
        var newConfig = MakeConfig(id: 10, price: 200000m);
        var dto = MakeDto(id: 10, price: 200000m);

        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(vehicleType);
        _repositoryMock.GetActiveConfigByVehicleTypeAsync(1).Returns((SubscriptionPriceConfig?)null);
        _repositoryMock.AddAsync(Arg.Any<SubscriptionPriceConfig>()).Returns(Task.CompletedTask);
        _repositoryMock.SaveChangesAsync().Returns(Task.FromResult(1));
        _repositoryMock.GetByIdAsync(10).Returns(newConfig);
        _mapperMock.Map<SubscriptionPriceConfigDto>(Arg.Any<SubscriptionPriceConfig>()).Returns(dto);

        var request = new CreateSubscriptionPriceConfigRequest { VehicleTypeId = 1, Price = 200000m };

        var result = await _service.CreateConfigAsync(request);

        Assert.NotNull(result);
        Assert.Equal(200000m, result.Price);
        Assert.True(result.IsActive);
        await _repositoryMock.Received(1).AddAsync(Arg.Any<SubscriptionPriceConfig>());
    }

    [Fact]
    public async Task CreateConfigAsync_ShouldDeactivateOldConfig_WhenActiveConfigExists()
    {
        var vehicleType = MakeVehicleType();
        var existingConfig = MakeConfig(id: 5, isActive: true);
        var newConfig = MakeConfig(id: 10, price: 150000m);
        var dto = MakeDto(id: 10, price: 150000m);

        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(vehicleType);
        _repositoryMock.GetActiveConfigByVehicleTypeAsync(1).Returns(existingConfig);
        _repositoryMock.AddAsync(Arg.Any<SubscriptionPriceConfig>()).Returns(Task.CompletedTask);
        _repositoryMock.SaveChangesAsync().Returns(Task.FromResult(1));
        _repositoryMock.GetByIdAsync(10).Returns(newConfig);
        _mapperMock.Map<SubscriptionPriceConfigDto>(Arg.Any<SubscriptionPriceConfig>()).Returns(dto);

        var request = new CreateSubscriptionPriceConfigRequest { VehicleTypeId = 1, Price = 150000m };

        var result = await _service.CreateConfigAsync(request);

        Assert.NotNull(result);
        Assert.False(existingConfig.IsActive);
        Assert.NotNull(existingConfig.EffectiveTo);
        _repositoryMock.Received(1).Update(existingConfig);
    }

    [Fact]
    public async Task CreateConfigAsync_ShouldThrow_WhenVehicleTypeNotFound()
    {
        _vehicleTypeRepoMock.GetByIdAsync(999).Returns((VehicleType?)null);

        var request = new CreateSubscriptionPriceConfigRequest { VehicleTypeId = 999, Price = 100000m };

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateConfigAsync(request));
        Assert.Equal("NOT_FOUND", ex.ErrorCode);
    }

    // =========================================================================
    // DeactivateConfigAsync
    // =========================================================================

    [Fact]
    public async Task DeactivateConfigAsync_ShouldReturnTrue_WhenConfigIsActive()
    {
        var config = MakeConfig(isActive: true);

        _repositoryMock.GetByIdAsync(1).Returns(config);

        var result = await _service.DeactivateConfigAsync(1);

        Assert.True(result);
        Assert.False(config.IsActive);
        Assert.NotNull(config.EffectiveTo);
        _repositoryMock.Received(1).Update(config);
        await _repositoryMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeactivateConfigAsync_ShouldReturnFalse_WhenConfigIsAlreadyInactive()
    {
        var config = MakeConfig(isActive: false);

        _repositoryMock.GetByIdAsync(1).Returns(config);

        var result = await _service.DeactivateConfigAsync(1);

        Assert.False(result);
        _repositoryMock.DidNotReceive().Update(Arg.Any<SubscriptionPriceConfig>());
    }

    [Fact]
    public async Task DeactivateConfigAsync_ShouldThrow_WhenConfigNotFound()
    {
        _repositoryMock.GetByIdAsync(999).Returns((SubscriptionPriceConfig?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeactivateConfigAsync(999));
        Assert.Equal("NOT_FOUND", ex.ErrorCode);
    }

    // =========================================================================
    // DeleteConfigAsync
    // =========================================================================

    [Fact]
    public async Task DeleteConfigAsync_ShouldDelete_WhenConfigExists()
    {
        var config = MakeConfig();

        _repositoryMock.GetByIdAsync(1).Returns(config);

        var result = await _service.DeleteConfigAsync(1);

        Assert.True(result);
        await _repositoryMock.Received(1).RemoveAsync(config);
        await _repositoryMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteConfigAsync_ShouldThrow_WhenConfigNotFound()
    {
        _repositoryMock.GetByIdAsync(999).Returns((SubscriptionPriceConfig?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteConfigAsync(999));
        Assert.Equal("NOT_FOUND", ex.ErrorCode);
    }

    // =========================================================================
    // GetAllConfigsAsync
    // =========================================================================

    [Fact]
    public async Task GetAllConfigsAsync_ShouldReturnConfigs()
    {
        var configs = new List<SubscriptionPriceConfig> { MakeConfig(), MakeConfig(id: 2, price: 1500000m) };
        var dtos = new List<SubscriptionPriceConfigDto> { MakeDto(), MakeDto(id: 2, price: 1500000m) };

        _repositoryMock.GetAllConfigsWithDetailsAsync(null, null).Returns(configs);
        _mapperMock.Map<IEnumerable<SubscriptionPriceConfigDto>>(configs).Returns(dtos);

        var result = (await _service.GetAllConfigsAsync(null, null)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllConfigsAsync_ShouldFilterByVehicleType()
    {
        var configs = new List<SubscriptionPriceConfig> { MakeConfig(vehicleTypeId: 1) };
        var dtos = new List<SubscriptionPriceConfigDto> { MakeDto(vehicleTypeId: 1) };

        _repositoryMock.GetAllConfigsWithDetailsAsync(1, null).Returns(configs);
        _mapperMock.Map<IEnumerable<SubscriptionPriceConfigDto>>(configs).Returns(dtos);

        var result = (await _service.GetAllConfigsAsync(1, null)).ToList();

        Assert.Single(result);
        await _repositoryMock.Received(1).GetAllConfigsWithDetailsAsync(1, null);
    }
}

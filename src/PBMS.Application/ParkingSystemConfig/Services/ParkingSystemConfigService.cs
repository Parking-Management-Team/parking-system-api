using PBMS.Application.Contracts;
using PBMS.Application.ParkingSystemConfig.DTOs;
using PBMS.Application.ParkingSystemConfig.Interfaces;
using ParkingSystemConfigEntity = PBMS.Domain.Entities.ParkingSystemConfig;

namespace PBMS.Application.ParkingSystemConfig.Services;

/// <summary>
/// Implements business logic for managing parking system configuration.
/// </summary>
public class ParkingSystemConfigService : IParkingSystemConfigService
{
    private readonly IParkingSystemConfigRepository _configRepository;

    public ParkingSystemConfigService(IParkingSystemConfigRepository configRepository)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParkingSystemConfigDto>> GetAllConfigsAsync()
    {
        var configs = await _configRepository.GetAllAsync();
        return configs.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<ParkingSystemConfigDto?> GetByKeyAsync(string key)
    {
        var config = await _configRepository.GetByKeyAsync(key);
        return config is null ? null : MapToDto(config);
    }

    /// <inheritdoc />
    public async Task<ParkingSystemConfigDto> UpsertConfigAsync(UpsertParkingSystemConfigRequest request)
    {
        var existing = await _configRepository.GetByKeyAsync(request.Key);

        if (existing is null)
        {
            existing = new ParkingSystemConfigEntity
            {
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = request.UpdatedBy
            };
        }
        else
        {
            existing.Value = request.Value;
            if (request.Description is not null)
            {
                existing.Description = request.Description;
            }
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = request.UpdatedBy;
        }

        await _configRepository.UpsertAsync(existing);
        await _configRepository.SaveChangesAsync();

        return MapToDto(existing);
    }

    /// <inheritdoc />
    public async Task<int> GetIntConfigAsync(string key, int defaultValue)
    {
        var config = await _configRepository.GetByKeyAsync(key);
        if (config is null)
        {
            return defaultValue;
        }

        return int.TryParse(config.Value, out var parsed) ? parsed : defaultValue;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static ParkingSystemConfigDto MapToDto(ParkingSystemConfigEntity config) => new()
    {
        Key = config.Key,
        Value = config.Value,
        Description = config.Description,
        UpdatedAt = config.UpdatedAt,
        UpdatedBy = config.UpdatedBy
    };
}

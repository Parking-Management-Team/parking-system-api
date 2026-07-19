using PBMS.Application.ParkingSystemConfig.DTOs;

namespace PBMS.Application.ParkingSystemConfig.Interfaces;

/// <summary>
/// Service interface for managing parking system configuration (key-value store).
/// </summary>
public interface IParkingSystemConfigService
{
    /// <summary>
    /// Returns all system configuration entries.
    /// </summary>
    Task<IEnumerable<ParkingSystemConfigDto>> GetAllConfigsAsync();

    /// <summary>
    /// Returns a single configuration entry by key, or null if not found.
    /// </summary>
    /// <param name="key">The configuration key to look up.</param>
    Task<ParkingSystemConfigDto?> GetByKeyAsync(string key);

    /// <summary>
    /// Creates or updates a configuration entry.
    /// </summary>
    /// <param name="request">The upsert request containing key, value, and metadata.</param>
    Task<ParkingSystemConfigDto> UpsertConfigAsync(UpsertParkingSystemConfigRequest request);

    /// <summary>
    /// Reads a config value as an integer. Returns the provided default if the key
    /// does not exist or the stored value cannot be parsed as an integer.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">Fallback value when the key is missing or invalid.</param>
    Task<int> GetIntConfigAsync(string key, int defaultValue);
}

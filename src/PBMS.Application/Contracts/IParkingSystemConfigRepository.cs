using ParkingSystemConfigEntity = PBMS.Domain.Entities.ParkingSystemConfig;

namespace PBMS.Application.Contracts;

/// <summary>
/// Repository interface for accessing and persisting ParkingSystemConfig entries.
/// </summary>
public interface IParkingSystemConfigRepository
{
    /// <summary>
    /// Returns a single config entry by key, or null if not found.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    Task<ParkingSystemConfigEntity?> GetByKeyAsync(string key);

    /// <summary>
    /// Returns all configuration entries.
    /// </summary>
    Task<IEnumerable<ParkingSystemConfigEntity>> GetAllAsync();

    /// <summary>
    /// Creates or updates a configuration entry in the database.
    /// </summary>
    /// <param name="config">The config entity to upsert.</param>
    Task UpsertAsync(ParkingSystemConfigEntity config);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}

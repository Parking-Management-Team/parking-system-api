using PBMS.Domain.Entities;

namespace PBMS.Application.Vehicle.Interfaces;

/// <summary>
/// Repository interface for Vehicle entity operations.
/// </summary>
public interface IVehicleRepository
{
    Task<IEnumerable<PBMS.Domain.Entities.Vehicle>> GetAllAsync();

    Task<IEnumerable<PBMS.Domain.Entities.Vehicle>> GetByAccountIdAsync(int accountId);

    Task<PBMS.Domain.Entities.Vehicle?> GetByIdAsync(int id);

    Task<bool> AccountExistsAsync(int accountId);

    Task<bool> LicensePlateExistsAsync(string licensePlate, int? excludeId = null);

    Task<bool> HasActiveParkingSessionAsync(int vehicleId);

    Task<PBMS.Domain.Entities.Vehicle> AddAsync(PBMS.Domain.Entities.Vehicle vehicle);

    Task<PBMS.Domain.Entities.Vehicle> UpdateAsync(PBMS.Domain.Entities.Vehicle vehicle);
}

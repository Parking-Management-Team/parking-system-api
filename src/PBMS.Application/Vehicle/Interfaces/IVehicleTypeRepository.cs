using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.Vehicle.Interfaces;

/// <summary>
/// Repository interface for VehicleType entity operations.
/// </summary>
public interface IVehicleTypeRepository
{
    /// <summary>
    /// Get all vehicle types.
    /// </summary>
    /// <returns>List of all vehicle types.</returns>
    Task<IEnumerable<VehicleType>> GetAllAsync();

    /// <summary>
    /// Get a vehicle type by ID.
    /// </summary>
    /// <param name="id">Vehicle type ID.</param>
    /// <returns>Vehicle type or null if not found.</returns>
    Task<VehicleType?> GetByIdAsync(int id);

    /// <summary>
    /// Check if a vehicle type name already exists.
    /// </summary>
    /// <param name="name">Vehicle type name.</param>
    /// <param name="excludeId">Exclude a specific ID from the check (for updates).</param>
    /// <returns>True if name exists, false otherwise.</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Add a new vehicle type.
    /// </summary>
    /// <param name="vehicleType">Vehicle type to add.</param>
    /// <returns>The added vehicle type with generated ID.</returns>
    Task<VehicleType> AddAsync(VehicleType vehicleType);

    /// <summary>
    /// Update an existing vehicle type.
    /// </summary>
    /// <param name="vehicleType">Vehicle type to update.</param>
    /// <returns>The updated vehicle type.</returns>
    Task<VehicleType> UpdateAsync(VehicleType vehicleType);

    /// <summary>
    /// Delete a vehicle type by ID.
    /// </summary>
    /// <param name="id">Vehicle type ID.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Check if a vehicle type is used in any active parking sessions.
    /// </summary>
    /// <param name="vehicleTypeId">Vehicle type ID.</param>
    /// <returns>True if used, false otherwise.</returns>
    Task<bool> IsUsedInSessionsAsync(int vehicleTypeId);

    /// <summary>
    /// Check if a vehicle type is used in any active bookings.
    /// </summary>
    /// <param name="vehicleTypeId">Vehicle type ID.</param>
    /// <returns>True if used, false otherwise.</returns>
    Task<bool> IsUsedInBookingsAsync(int vehicleTypeId);
}

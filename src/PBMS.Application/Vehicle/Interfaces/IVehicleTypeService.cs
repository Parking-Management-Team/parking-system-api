using PBMS.Application.Common;
using PBMS.Application.Vehicle.DTOs;

namespace PBMS.Application.Vehicle.Interfaces;

/// <summary>
/// Service interface for managing vehicle types.
/// </summary>
public interface IVehicleTypeService
{
    /// <summary>
    /// Get all vehicle types.
    /// </summary>
    /// <returns>List of all vehicle types with their status.</returns>
    Task<BaseResponse<IEnumerable<VehicleTypeDto>>> GetAllAsync();

    /// <summary>
    /// Get a specific vehicle type by ID.
    /// </summary>
    /// <param name="id">Vehicle type ID.</param>
    /// <returns>Vehicle type details or error if not found.</returns>
    Task<BaseResponse<VehicleTypeDto>> GetByIdAsync(int id);

    /// <summary>
    /// Create a new vehicle type.
    /// </summary>
    /// <param name="createDto">Data for creating the vehicle type.</param>
    /// <returns>Created vehicle type or error if name already exists.</returns>
    Task<BaseResponse<VehicleTypeDto>> CreateAsync(CreateVehicleTypeDto createDto);

    /// <summary>
    /// Update an existing vehicle type.
    /// </summary>
    /// <param name="id">Vehicle type ID to update.</param>
    /// <param name="updateDto">Data to update.</param>
    /// <returns>Updated vehicle type or error if validation fails.</returns>
    Task<BaseResponse<VehicleTypeDto>> UpdateAsync(int id, UpdateVehicleTypeDto updateDto);

    /// <summary>
    /// Delete a vehicle type.
    /// </summary>
    /// <param name="id">Vehicle type ID to delete.</param>
    /// <returns>Success response or error if vehicle type is in use.</returns>
    Task<BaseResponse<object>> DeleteAsync(int id);
}

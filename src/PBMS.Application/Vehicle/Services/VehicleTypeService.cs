using PBMS.Application.Common;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.Vehicle.Services;

/// <summary>
/// Service for managing vehicle types with business logic validation.
/// </summary>
public class VehicleTypeService : IVehicleTypeService
{
    private readonly IVehicleTypeRepository _repository;

    public VehicleTypeService(IVehicleTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<BaseResponse<IEnumerable<VehicleTypeDto>>> GetAllAsync()
    {
        try
        {
            var vehicleTypes = await _repository.GetAllAsync();
            var dtos = vehicleTypes.Select(vt => MapToDto(vt)).ToList();

            return BaseResponse<IEnumerable<VehicleTypeDto>>.Ok(
                dtos,
                $"Get {dtos.Count} vehicle types successfully."
            );
        }
        catch (Exception ex)
        {
            return BaseResponse<IEnumerable<VehicleTypeDto>>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while fetching vehicle types: {ex.Message}"
            );
        }
    }

    public async Task<BaseResponse<VehicleTypeDto>> GetByIdAsync(int id)
    {
        try
        {
            var vehicleType = await _repository.GetByIdAsync(id);
            if (vehicleType == null)
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NOT_FOUND",
                    $"Vehicle type with ID {id} not found."
                );
            }

            return BaseResponse<VehicleTypeDto>.Ok(
                MapToDto(vehicleType),
                "Vehicle type information retrieved successfully."
            );
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while fetching vehicle type information: {ex.Message}"
            );
        }
    }

    public async Task<BaseResponse<VehicleTypeDto>> CreateAsync(CreateVehicleTypeDto createDto)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "INVALID_NAME",
                    "Vehicle type name cannot be empty."
                );
            }

            // Check for duplicate name
            if (await _repository.NameExistsAsync(createDto.Name))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NAME_EXISTS",
                    $"Vehicle type '{createDto.Name}' already exists in the system."
                );
            }

            var vehicleType = new VehicleType
            {
                Name = createDto.Name.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(vehicleType);
            return BaseResponse<VehicleTypeDto>.Ok(
                MapToDto(created),
                $"Created vehicle type '{created.Name}' successfully."
            );
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while creating vehicle type: {ex.Message}"
            );
        }
    }

    public async Task<BaseResponse<VehicleTypeDto>> UpdateAsync(int id, UpdateVehicleTypeDto updateDto)
    {
        try
        {
            var vehicleType = await _repository.GetByIdAsync(id);
            if (vehicleType == null)
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NOT_FOUND",
                    $"Vehicle type with ID {id} not found."
                );
            }

            // Validation
            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "INVALID_NAME",
                    "Vehicle type name cannot be empty."
                );
            }

            // Check for duplicate name (excluding current vehicle type)
            if (await _repository.NameExistsAsync(updateDto.Name, id))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NAME_EXISTS",
                    $"Vehicle type '{updateDto.Name}' already exists in the system."
                );
            }

            vehicleType.Name = updateDto.Name.Trim();
            vehicleType.IsActive = updateDto.IsActive;

            var updated = await _repository.UpdateAsync(vehicleType);
            return BaseResponse<VehicleTypeDto>.Ok(
                MapToDto(updated),
                $"Updated vehicle type '{updated.Name}' successfully."
            );
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while updating vehicle type: {ex.Message}"
            );
        }
    }

    public async Task<BaseResponse<object>> DeleteAsync(int id)
    {
        try
        {
            var vehicleType = await _repository.GetByIdAsync(id);
            if (vehicleType == null)
            {
                return BaseResponse<object>.Fail(
                    "NOT_FOUND",
                    $"Vehicle type with ID {id} not found."
                );
            }

            // Check if used in parking sessions
            if (await _repository.IsUsedInSessionsAsync(id))
            {
                return BaseResponse<object>.Fail(
                    "IN_USE_SESSIONS",
                    $"Cannot delete vehicle type '{vehicleType.Name}' because it is currently in use in active parking sessions."
                );
            }

            // Check if used in bookings
            if (await _repository.IsUsedInBookingsAsync(id))
            {
                return BaseResponse<object>.Fail(
                    "IN_USE_BOOKINGS",
                    $"Cannot delete vehicle type '{vehicleType.Name}' because it is currently in use in active bookings."
                );
            }

            await _repository.DeleteAsync(id);
            return BaseResponse<object>.Ok(
                null,
                $"Deleted vehicle type '{vehicleType.Name}' successfully."
            );
        }
        catch (Exception ex)
        {
            return BaseResponse<object>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while deleting vehicle type: {ex.Message}"
            );
        }
    }

    private VehicleTypeDto MapToDto(VehicleType vehicleType)
    {
        return new VehicleTypeDto
        {
            Id = vehicleType.Id,
            Name = vehicleType.Name,
            IsActive = vehicleType.IsActive,
            Status = vehicleType.IsActive ? "Active" : "Inactive",
            CreatedAt = vehicleType.CreatedAt
        };
    }
}

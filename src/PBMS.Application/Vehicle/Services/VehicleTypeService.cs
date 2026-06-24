using PBMS.Application.Common;
using PBMS.Application.Contracts;
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
    private readonly IPricingPolicyRepository _pricingPolicyRepository;
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        VehicleType.StatusActive,
        VehicleType.StatusInactive
    };

    public VehicleTypeService(IVehicleTypeRepository repository, IPricingPolicyRepository pricingPolicyRepository)
    {
        _repository = repository;
        _pricingPolicyRepository = pricingPolicyRepository;
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
            var validationError = ValidateVehicleType(createDto.Name, createDto.Description, createDto.VehicleTypeStatus);
            if (validationError != null)
            {
                return validationError;
            }

            var normalizedName = createDto.Name.Trim();
            if (await _repository.NameExistsAsync(normalizedName))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NAME_EXISTS",
                    $"Vehicle type '{normalizedName}' already exists in the system."
                );
            }

            var requestedStatus = NormalizeStatus(createDto.VehicleTypeStatus);
            if (string.Equals(requestedStatus, VehicleType.StatusActive, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "PRICING_POLICY_REQUIRED",
                    "A new vehicle type cannot be created as ACTIVE directly because it needs an active Pricing Policy. Please create it as INACTIVE first, configure its Pricing Policy, and then activate it."
                );
            }

            if (createDto.BufferRatio < 0 || createDto.BufferRatio > 100)
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "INVALID_BUFFER_RATIO",
                    "Buffer ratio must be between 0 and 100."
                );
            }

            var vehicleType = new VehicleType
            {
                TypeName = normalizedName,
                Description = NormalizeDescription(createDto.Description),
                VehicleTypeStatus = requestedStatus,
                BufferRatio = createDto.BufferRatio
            };

            var created = await _repository.AddAsync(vehicleType);
            return BaseResponse<VehicleTypeDto>.Ok(
                MapToDto(created),
                $"Created vehicle type '{created.TypeName}' successfully."
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

            var requestedStatus = updateDto.VehicleTypeStatus;

            var validationError = ValidateVehicleType(updateDto.Name, updateDto.Description, requestedStatus);
            if (validationError != null)
            {
                return validationError;
            }

            var normalizedName = updateDto.Name.Trim();
            if (await _repository.NameExistsAsync(normalizedName, id))
            {
                return BaseResponse<VehicleTypeDto>.Fail(
                    "NAME_EXISTS",
                    $"Vehicle type '{normalizedName}' already exists in the system."
                );
            }

            var targetStatus = NormalizeStatus(requestedStatus);
            if (string.Equals(targetStatus, VehicleType.StatusActive, StringComparison.OrdinalIgnoreCase))
            {
                var activePolicy = await _pricingPolicyRepository.GetActivePolicyAsync(id, DateTime.UtcNow);
                if (activePolicy == null)
                {
                    return BaseResponse<VehicleTypeDto>.Fail(
                        "PRICING_POLICY_REQUIRED",
                        $"Vehicle type '{normalizedName}' cannot be activated because it does not have any active Pricing Policy configured. Please create a Pricing Policy for this vehicle type first."
                    );
                }
            }

            vehicleType.TypeName = normalizedName;
            vehicleType.Description = NormalizeDescription(updateDto.Description);
            vehicleType.VehicleTypeStatus = targetStatus;
            if (updateDto.BufferRatio.HasValue)
            {
                if (updateDto.BufferRatio.Value < 0 || updateDto.BufferRatio.Value > 100)
                {
                    return BaseResponse<VehicleTypeDto>.Fail(
                        "INVALID_BUFFER_RATIO",
                        "Buffer ratio must be between 0 and 100."
                    );
                }
                vehicleType.BufferRatio = updateDto.BufferRatio.Value;
            }

            var updated = await _repository.UpdateAsync(vehicleType);
            return BaseResponse<VehicleTypeDto>.Ok(
                MapToDto(updated),
                $"Updated vehicle type '{updated.TypeName}' successfully."
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
                    $"Cannot delete vehicle type '{vehicleType.TypeName}' because it is currently in use in active parking sessions."
                );
            }

            // Check if used in bookings
            if (await _repository.IsUsedInBookingsAsync(id))
            {
                return BaseResponse<object>.Fail(
                    "IN_USE_BOOKINGS",
                    $"Cannot delete vehicle type '{vehicleType.TypeName}' because it is currently in use in active bookings."
                );
            }

            vehicleType.VehicleTypeStatus = VehicleType.StatusInactive;
            await _repository.UpdateAsync(vehicleType);
            return BaseResponse<object>.Ok(
                null,
                $"Archived vehicle type '{vehicleType.TypeName}' successfully."
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
            Name = vehicleType.TypeName,
            Description = vehicleType.Description,
            VehicleTypeStatus = vehicleType.VehicleTypeStatus,
            BufferRatio = vehicleType.BufferRatio
        };
    }

    private static BaseResponse<VehicleTypeDto>? ValidateVehicleType(
        string name,
        string? description,
        string? vehicleTypeStatus)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INVALID_NAME",
                "Vehicle type name cannot be empty."
            );
        }

        if (name.Trim().Length > 50)
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INVALID_NAME",
                "Vehicle type name cannot exceed 50 characters."
            );
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 100)
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INVALID_DESCRIPTION",
                "Vehicle type description cannot exceed 100 characters."
            );
        }

        if (!string.IsNullOrWhiteSpace(vehicleTypeStatus)
            && !AllowedStatuses.Contains(vehicleTypeStatus.Trim()))
        {
            return BaseResponse<VehicleTypeDto>.Fail(
                "INVALID_STATUS",
                "Vehicle type status must be ACTIVE or INACTIVE."
            );
        }

        return null;
    }

    private static string NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? VehicleType.StatusActive
            : status.Trim().ToUpperInvariant();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}

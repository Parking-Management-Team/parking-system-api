using PBMS.Application.Common;
using PBMS.Application.Vehicle.DTOs;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Application.Vehicle.Services;

/// <summary>
/// Service for managing vehicles according to FR-002.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleTypeRepository _vehicleTypeRepository;

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PBMS.Domain.Entities.Vehicle.StatusActive,
        PBMS.Domain.Entities.Vehicle.StatusInactive,
        PBMS.Domain.Entities.Vehicle.StatusPending,
        PBMS.Domain.Entities.Vehicle.StatusSuspended,
        PBMS.Domain.Entities.Vehicle.StatusArchived
    };

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IVehicleTypeRepository vehicleTypeRepository)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
    }

    public async Task<BaseResponse<IEnumerable<VehicleDto>>> GetAllAsync()
    {
        try
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            var dtos = vehicles.Select(MapToDto).ToList();
            return BaseResponse<IEnumerable<VehicleDto>>.Ok(dtos, $"Get {dtos.Count} vehicles successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<IEnumerable<VehicleDto>>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while fetching vehicles: {ex.Message}");
        }
    }

    public async Task<BaseResponse<IEnumerable<VehicleDto>>> GetByAccountIdAsync(int accountId)
    {
        try
        {
            if (accountId <= 0)
            {
                return BaseResponse<IEnumerable<VehicleDto>>.Fail("INVALID_ACCOUNT", "Account ID must be greater than 0.");
            }

            if (!await _vehicleRepository.AccountExistsAsync(accountId))
            {
                return BaseResponse<IEnumerable<VehicleDto>>.Fail("ACCOUNT_NOT_FOUND", $"Account with ID {accountId} not found.");
            }

            var vehicles = await _vehicleRepository.GetByAccountIdAsync(accountId);
            return BaseResponse<IEnumerable<VehicleDto>>.Ok(
                vehicles.Select(MapToDto).ToList(),
                "Vehicles retrieved successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<IEnumerable<VehicleDto>>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while fetching account vehicles: {ex.Message}");
        }
    }

    public async Task<BaseResponse<VehicleDto>> GetByIdAsync(int id)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return BaseResponse<VehicleDto>.Fail("NOT_FOUND", $"Vehicle with ID {id} not found.");
            }

            return BaseResponse<VehicleDto>.Ok(MapToDto(vehicle), "Vehicle information retrieved successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while fetching vehicle information: {ex.Message}");
        }
    }

    public async Task<BaseResponse<VehicleDto>> CreateAsync(CreateVehicleDto createDto)
    {
        try
        {
            var validation = await ValidateVehicleInputAsync(
                createDto.AccountId,
                createDto.VehicleTypeId,
                createDto.LicensePlate,
                createDto.VehicleStatus);
            if (!validation.Success)
            {
                return validation;
            }

            var normalizedPlate = NormalizeLicensePlate(createDto.LicensePlate);
            if (await _vehicleRepository.LicensePlateExistsAsync(normalizedPlate))
            {
                return BaseResponse<VehicleDto>.Fail(
                    "LICENSE_PLATE_EXISTS",
                    $"Vehicle license plate '{createDto.LicensePlate.Trim()}' already exists in the system.");
            }

            var vehicle = new PBMS.Domain.Entities.Vehicle
            {
                AccountId = createDto.AccountId,
                VehicleTypeId = createDto.VehicleTypeId,
                LicensePlate = createDto.LicensePlate.Trim(),
                RegisteredDay = createDto.RegisteredDay,
                VehicleStatus = NormalizeStatus(createDto.VehicleStatus)
            };

            var created = await _vehicleRepository.AddAsync(vehicle);
            return BaseResponse<VehicleDto>.Ok(MapToDto(created), "Created vehicle successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while creating vehicle: {ex.Message}");
        }
    }

    public async Task<BaseResponse<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto updateDto)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return BaseResponse<VehicleDto>.Fail("NOT_FOUND", $"Vehicle with ID {id} not found.");
            }

            var validation = await ValidateVehicleInputAsync(
                updateDto.AccountId,
                updateDto.VehicleTypeId,
                updateDto.LicensePlate,
                updateDto.VehicleStatus);
            if (!validation.Success)
            {
                return validation;
            }

            var normalizedPlate = NormalizeLicensePlate(updateDto.LicensePlate);
            if (await _vehicleRepository.LicensePlateExistsAsync(normalizedPlate, id))
            {
                return BaseResponse<VehicleDto>.Fail(
                    "LICENSE_PLATE_EXISTS",
                    $"Vehicle license plate '{updateDto.LicensePlate.Trim()}' already exists in the system.");
            }

            vehicle.AccountId = updateDto.AccountId;
            vehicle.VehicleTypeId = updateDto.VehicleTypeId;
            vehicle.LicensePlate = updateDto.LicensePlate.Trim();
            vehicle.RegisteredDay = updateDto.RegisteredDay;
            vehicle.VehicleStatus = NormalizeStatus(updateDto.VehicleStatus);

            var updated = await _vehicleRepository.UpdateAsync(vehicle);
            return BaseResponse<VehicleDto>.Ok(MapToDto(updated), "Updated vehicle successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while updating vehicle: {ex.Message}");
        }
    }

    public async Task<BaseResponse<VehicleDto>> ArchiveAsync(int id)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return BaseResponse<VehicleDto>.Fail("NOT_FOUND", $"Vehicle with ID {id} not found.");
            }

            if (await _vehicleRepository.HasActiveParkingSessionAsync(id))
            {
                return BaseResponse<VehicleDto>.Fail(
                    "VEHICLE_IN_ACTIVE_SESSION",
                    "Cannot archive a vehicle while it has an active parking session.");
            }

            vehicle.VehicleStatus = PBMS.Domain.Entities.Vehicle.StatusArchived;
            var archived = await _vehicleRepository.UpdateAsync(vehicle);
            return BaseResponse<VehicleDto>.Ok(MapToDto(archived), "Archived vehicle successfully.");
        }
        catch (Exception ex)
        {
            return BaseResponse<VehicleDto>.Fail(
                "INTERNAL_ERROR",
                $"Error occurred while archiving vehicle: {ex.Message}");
        }
    }

    public static string NormalizeLicensePlate(string licensePlate)
    {
        return new string(licensePlate
            .Trim()
            .ToUpperInvariant()
            .Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.')
            .ToArray());
    }

    private async Task<BaseResponse<VehicleDto>> ValidateVehicleInputAsync(
        int? accountId,
        int vehicleTypeId,
        string licensePlate,
        string? vehicleStatus)
    {
        if (accountId.HasValue && accountId.Value <= 0)
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_ACCOUNT", "Account ID must be greater than 0.");
        }

        if (accountId.HasValue && !await _vehicleRepository.AccountExistsAsync(accountId.Value))
        {
            return BaseResponse<VehicleDto>.Fail("ACCOUNT_NOT_FOUND", $"Account with ID {accountId.Value} not found.");
        }

        if (vehicleTypeId <= 0)
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_VEHICLE_TYPE", "Vehicle type ID must be greater than 0.");
        }

        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(vehicleTypeId);
        if (vehicleType == null)
        {
            return BaseResponse<VehicleDto>.Fail("VEHICLE_TYPE_NOT_FOUND", $"Vehicle type with ID {vehicleTypeId} not found.");
        }

        if (!string.Equals(vehicleType.VehicleTypeStatus, VehicleType.StatusActive, StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<VehicleDto>.Fail("VEHICLE_TYPE_INACTIVE", "Vehicle type is not active.");
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_LICENSE_PLATE", "License plate cannot be empty.");
        }

        if (licensePlate.Trim().Length > 20)
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_LICENSE_PLATE", "License plate cannot exceed 20 characters.");
        }

        if (NormalizeLicensePlate(licensePlate).Length == 0)
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_LICENSE_PLATE", "License plate must contain letters or numbers.");
        }

        if (NormalizeLicensePlate(licensePlate).Length > 20)
        {
            return BaseResponse<VehicleDto>.Fail("INVALID_LICENSE_PLATE", "Normalized license plate cannot exceed 20 characters.");
        }

        if (!string.IsNullOrWhiteSpace(vehicleStatus) && !AllowedStatuses.Contains(vehicleStatus.Trim()))
        {
            return BaseResponse<VehicleDto>.Fail(
                "INVALID_STATUS",
                "Vehicle status must be ACTIVE, INACTIVE, PENDING, SUSPENDED, or ARCHIVED.");
        }

        return BaseResponse<VehicleDto>.Ok(null);
    }

    private static string NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? PBMS.Domain.Entities.Vehicle.StatusActive
            : status.Trim().ToUpperInvariant();
    }

    private static VehicleDto MapToDto(PBMS.Domain.Entities.Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            AccountId = vehicle.AccountId,
            VehicleTypeId = vehicle.VehicleTypeId,
            VehicleTypeName = vehicle.VehicleType?.TypeName,
            LicensePlate = vehicle.LicensePlate,
            RegisteredDay = vehicle.RegisteredDay,
            VehicleStatus = vehicle.VehicleStatus
        };
    }
}

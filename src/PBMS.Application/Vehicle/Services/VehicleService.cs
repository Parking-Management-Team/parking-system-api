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

            // Check if vehicle with same license plate already exists
            var existingVehicle = await _vehicleRepository.GetByLicensePlateAsync(normalizedPlate);
            if (existingVehicle != null)
            {
                if (existingVehicle.AccountId == null)
                {
                    // Vehicle is a guest/walk-in vehicle (no owner). Claim it by assigning AccountId.
                    existingVehicle.AccountId = createDto.AccountId;
                    existingVehicle.VehicleTypeId = createDto.VehicleTypeId;
                    existingVehicle.RegisteredDay = createDto.RegisteredDay ?? existingVehicle.RegisteredDay;
                    existingVehicle.VehicleStatus = NormalizeStatus(createDto.VehicleStatus);

                    var claimed = await _vehicleRepository.UpdateAsync(existingVehicle);
                    return BaseResponse<VehicleDto>.Ok(MapToDto(claimed), "Vehicle claimed and linked to account successfully.");
                }

                // Vehicle already belongs to another account
                return BaseResponse<VehicleDto>.Fail(
                    "LICENSE_PLATE_EXISTS",
                    $"Vehicle license plate '{createDto.LicensePlate.Trim()}' already exists in the system.");
            }

            var vehicle = new PBMS.Domain.Entities.Vehicle
            {
                AccountId = createDto.AccountId,
                VehicleTypeId = createDto.VehicleTypeId,
                LicensePlate = normalizedPlate,
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
            vehicle.LicensePlate = normalizedPlate;
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

    public static string DetectVehicleTypeFromPlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate)) return "Car";
        var clean = new string(licensePlate
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (clean.Length < 3) return "Car";

        var match = System.Text.RegularExpressions.Regex.Match(clean, @"^(.*?)(\d{4,5})$");
        if (!match.Success) return "Car";

        var prefix = match.Groups[1].Value;

        // 1. Motorcycle standard: 2 digits + 1 letter + 1 digit (e.g., 29G1, 59T2)
        if (System.Text.RegularExpressions.Regex.IsMatch(prefix, @"^\d{2}[A-Z]\d$"))
        {
            return "Motorcycle";
        }

        // 2. Motorcycle electric / under 50cc: 2 digits + 2 letters (e.g., 29AA, 59AB, 29MD)
        // Excluding special car prefixes: LD, DA, MK, HC, NG, QT, NN, KT
        if (System.Text.RegularExpressions.Regex.IsMatch(prefix, @"^\d{2}[A-Z]{2}$"))
        {
            var letters = prefix.Substring(2);
            var carSpecialLetters = new[] { "LD", "DA", "MK", "HC", "NG", "QT", "NN", "KT" };
            if (carSpecialLetters.Contains(letters))
            {
                return "Car";
            }
            return "Motorcycle";
        }

        return "Car";
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

        // Validate vehicle type against license plate format
        var detectedCategory = DetectVehicleTypeFromPlate(licensePlate);
        var selectedTypeName = vehicleType.TypeName ?? "";
        bool isSelectedTypeMotorcycle = selectedTypeName.Contains("Motor", StringComparison.OrdinalIgnoreCase) || 
                                       selectedTypeName.Contains("Bike", StringComparison.OrdinalIgnoreCase) || 
                                       selectedTypeName.Contains("Scoot", StringComparison.OrdinalIgnoreCase) ||
                                       selectedTypeName.Contains("máy", StringComparison.OrdinalIgnoreCase);

        if (detectedCategory == "Motorcycle" && !isSelectedTypeMotorcycle)
        {
            return BaseResponse<VehicleDto>.Fail("VEHICLE_TYPE_MISMATCH", "This license plate is for a motorcycle. Please select a motorcycle vehicle type.");
        }
        else if (detectedCategory == "Car" && isSelectedTypeMotorcycle)
        {
            return BaseResponse<VehicleDto>.Fail("VEHICLE_TYPE_MISMATCH", "This license plate is for a car/truck. Please select a car or non-motorcycle vehicle type.");
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

        // ── Mục 2: Validate định dạng biển số Việt Nam chuẩn ────────────────
        // Regex: 2 chữ số + 1-2 chữ hoa + 4-5 chữ số  (sau khi đã normalize)
        // Ví dụ hợp lệ: 30A12345, 59AB1234, 29G11234, 51F12345
        var plateFormatRegex = new System.Text.RegularExpressions.Regex(@"^\d{2}[A-Z]{1,2}\d{4,5}$");
        if (!plateFormatRegex.IsMatch(NormalizeLicensePlate(licensePlate)))
        {
            return BaseResponse<VehicleDto>.Fail(
                "INVALID_LICENSE_PLATE_FORMAT",
                "License plate does not match Vietnamese standard format. Valid examples: 30A-123.45 or 59AB-1234.");
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

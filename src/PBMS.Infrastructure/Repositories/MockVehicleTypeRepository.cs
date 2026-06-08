using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// In-memory repository for Swagger/manual testing before the database schema is ready.
/// </summary>
public class MockVehicleTypeRepository : IVehicleTypeRepository
{
    private static readonly object SyncRoot = new();
    private static readonly List<VehicleType> VehicleTypes =
    [
        new VehicleType
        {
            Id = 1,
            Name = VehicleType.MotorcycleTypeName,
            Description = "Managed by zone capacity. Slot is not required for booking or monthly card.",
            VehicleTypeStatus = VehicleType.StatusActive
        },
        new VehicleType
        {
            Id = 2,
            Name = VehicleType.CarTypeName,
            Description = "Managed by slot for booking and monthly card.",
            VehicleTypeStatus = VehicleType.StatusActive
        }
    ];

    public Task<IEnumerable<VehicleType>> GetAllAsync()
    {
        lock (SyncRoot)
        {
            return Task.FromResult(VehicleTypes.OrderBy(vt => vt.Name).Select(Clone).AsEnumerable());
        }
    }

    public Task<VehicleType?> GetByIdAsync(int id)
    {
        lock (SyncRoot)
        {
            var vehicleType = VehicleTypes.FirstOrDefault(vt => vt.Id == id);
            return Task.FromResult(vehicleType == null ? null : Clone(vehicleType));
        }
    }

    public Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        lock (SyncRoot)
        {
            var normalizedName = name.Trim();
            var exists = VehicleTypes.Any(vt =>
                vt.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)
                && (!excludeId.HasValue || vt.Id != excludeId.Value));

            return Task.FromResult(exists);
        }
    }

    public Task<VehicleType> AddAsync(VehicleType vehicleType)
    {
        lock (SyncRoot)
        {
            vehicleType.Id = VehicleTypes.Count == 0 ? 1 : VehicleTypes.Max(vt => vt.Id) + 1;
            VehicleTypes.Add(Clone(vehicleType));
            return Task.FromResult(Clone(vehicleType));
        }
    }

    public Task<VehicleType> UpdateAsync(VehicleType vehicleType)
    {
        lock (SyncRoot)
        {
            var index = VehicleTypes.FindIndex(vt => vt.Id == vehicleType.Id);
            if (index >= 0)
            {
                VehicleTypes[index] = Clone(vehicleType);
            }

            return Task.FromResult(Clone(vehicleType));
        }
    }

    public Task<bool> DeleteAsync(int id)
    {
        lock (SyncRoot)
        {
            var vehicleType = VehicleTypes.FirstOrDefault(vt => vt.Id == id);
            if (vehicleType == null)
            {
                return Task.FromResult(false);
            }

            VehicleTypes.Remove(vehicleType);
            return Task.FromResult(true);
        }
    }

    public Task<bool> IsUsedInSessionsAsync(int vehicleTypeId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsUsedInBookingsAsync(int vehicleTypeId)
    {
        return Task.FromResult(false);
    }

    private static VehicleType Clone(VehicleType vehicleType)
    {
        return new VehicleType
        {
            Id = vehicleType.Id,
            Name = vehicleType.Name,
            Description = vehicleType.Description,
            VehicleTypeStatus = vehicleType.VehicleTypeStatus
        };
    }
}

using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using PBMS.Application.Vehicle.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for VehicleType entity operations.
/// </summary>
public class VehicleTypeRepository : IVehicleTypeRepository
{
    private readonly AppDbContext _context;

    public VehicleTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleType>> GetAllAsync()
    {
        return await _context.VehicleTypes.OrderBy(vt => vt.Name).ToListAsync();
    }

    public async Task<VehicleType?> GetByIdAsync(int id)
    {
        return await _context.VehicleTypes.FindAsync(id);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var normalizedName = name.Trim().ToUpper();
        var query = _context.VehicleTypes.Where(vt => vt.Name.ToUpper() == normalizedName);
        
        if (excludeId.HasValue)
        {
            query = query.Where(vt => vt.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<VehicleType> AddAsync(VehicleType vehicleType)
    {
        await _context.VehicleTypes.AddAsync(vehicleType);
        await _context.SaveChangesAsync();
        return vehicleType;
    }

    public async Task<VehicleType> UpdateAsync(VehicleType vehicleType)
    {
        _context.VehicleTypes.Update(vehicleType);
        await _context.SaveChangesAsync();
        return vehicleType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicleType = await _context.VehicleTypes.FindAsync(id);
        if (vehicleType == null)
        {
            return false;
        }

        _context.VehicleTypes.Remove(vehicleType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUsedInSessionsAsync(int vehicleTypeId)
    {
        // Check if the vehicle type is used in any incomplete parking sessions
        return await _context.VehicleTypes
            .Where(vt => vt.Id == vehicleTypeId)
            .SelectMany(vt => _context.Vehicles.Where(v => v.VehicleTypeId == vt.Id))
            .SelectMany(v => _context.ParkingSessions.Where(ps => ps.VehicleId == v.Id && !ps.IsCompleted))
            .AnyAsync();
    }

    public async Task<bool> IsUsedInBookingsAsync(int vehicleTypeId)
    {
        // This is a placeholder for future implementation
        // Adjust based on actual Booking entity structure when available
        return false;
    }
}


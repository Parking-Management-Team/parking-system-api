using Microsoft.EntityFrameworkCore;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Vehicle entity operations.
/// </summary>
public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.VehicleType)
            .OrderBy(v => v.LicensePlate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetByAccountIdAsync(int accountId)
    {
        return await _context.Vehicles
            .Include(v => v.VehicleType)
            .Where(v => v.AccountId == accountId)
            .OrderBy(v => v.LicensePlate)
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.VehicleType)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<bool> AccountExistsAsync(int accountId)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == accountId);
    }

    public async Task<bool> LicensePlateExistsAsync(string licensePlate, int? excludeId = null)
    {
        var normalizedLicensePlate = NormalizeLicensePlate(licensePlate);
        var query = _context.Vehicles.Where(v =>
            v.LicensePlate.ToUpper().Replace(" ", "").Replace("-", "").Replace(".", "") == normalizedLicensePlate);

        if (excludeId.HasValue)
        {
            query = query.Where(v => v.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    private static string NormalizeLicensePlate(string licensePlate)
    {
        return new string(licensePlate
            .Trim()
            .ToUpperInvariant()
            .Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.')
            .ToArray());
    }

    public async Task<bool> HasActiveParkingSessionAsync(int vehicleId)
    {
        return await _context.ParkingSessions.AnyAsync(ps =>
            ps.VehicleId == vehicleId && ps.SessionStatus.ToUpper() == "ACTIVE");
    }

    public async Task<Vehicle> AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(vehicle.Id)) ?? vehicle;
    }

    public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(vehicle.Id)) ?? vehicle;
    }
}

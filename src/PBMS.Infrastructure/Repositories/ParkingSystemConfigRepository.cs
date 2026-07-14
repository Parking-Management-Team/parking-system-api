using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class ParkingSystemConfigRepository : IParkingSystemConfigRepository
{
    private readonly AppDbContext _context;

    public ParkingSystemConfigRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ParkingSystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.ParkingSystemConfigs.FindAsync(key);
    }

    public async Task<IEnumerable<ParkingSystemConfig>> GetAllAsync()
    {
        return await _context.ParkingSystemConfigs.ToListAsync();
    }

    public async Task UpsertAsync(ParkingSystemConfig config)
    {
        var existing = await _context.ParkingSystemConfigs.FindAsync(config.Key);
        if (existing == null)
        {
            _context.ParkingSystemConfigs.Add(config);
        }
        else
        {
            existing.Value = config.Value;
            existing.Description = config.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = config.UpdatedBy;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

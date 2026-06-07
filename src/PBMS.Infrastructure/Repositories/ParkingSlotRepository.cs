using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai repository cho entity ParkingSlot.
/// </summary>
public class ParkingSlotRepository : BaseRepository<ParkingSlot>, IParkingSlotRepository
{
    private readonly AppDbContext _dbContext;

    public ParkingSlotRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ParkingSlot>> GetSlotsByZoneIdAsync(int zoneId)
    {
        return await FindAsync(s => s.ZoneId == zoneId);
    }

    public async Task<bool> SlotCodeExistsAsync(string slotCode)
    {
        return await AnyAsync(s => s.Code.ToLower() == slotCode.ToLower());
    }

    public async Task<ParkingSlot?> GetSlotWithDetailsAsync(int id)
    {
        return await _dbContext.Set<ParkingSlot>()
            .Include(s => s.Zone)
            .Include(s => s.VehicleType)
            .Include(s => s.ParkingSessions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}

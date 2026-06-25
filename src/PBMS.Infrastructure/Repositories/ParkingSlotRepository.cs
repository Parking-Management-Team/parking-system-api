using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
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
        return await _dbContext.Set<ParkingSlot>()
            .Include(s => s.ParkingSessions)
            .Include(s => s.MonthlySubscriptions)
                .ThenInclude(ms => ms.Vehicle)
            .Where(s => s.ZoneId == zoneId)
            .ToListAsync();
    }

    public async Task<bool> SlotCodeExistsAsync(string slotCode)
    {
        return await AnyAsync(s => s.Code.ToLower() == slotCode.ToLower());
    }

    public async Task<ParkingSlot?> GetSlotWithDetailsAsync(int id)
    {
        return await _dbContext.Set<ParkingSlot>()
            .Include(s => s.Zone)
                .ThenInclude(z => z.Floor)
            .Include(s => s.VehicleType)
            .Include(s => s.ParkingSessions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ParkingSlot?> FindAvailableMonthlySlotAsync(int buildingId, int vehicleTypeId)
    {
        return await _dbContext.Set<ParkingSlot>()
            .Include(s => s.Zone)
            .Include(s => s.MonthlySubscriptions)
            .FirstOrDefaultAsync(s => 
                s.Zone.Floor.BuildingId == buildingId &&
                s.Zone.AccessType == ZoneAccessType.Monthly &&
                s.VehicleTypeId == vehicleTypeId &&
                s.Status != SlotStatus.Blocked &&
                !s.MonthlySubscriptions.Any(ms => ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active || ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Pending));
    }
}


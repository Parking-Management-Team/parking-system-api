using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai repository cho entity Floor.
/// </summary>
public class FloorRepository : BaseRepository<Floor>, IFloorRepository
{
    private readonly AppDbContext _dbContext;

    public FloorRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Floor>> GetFloorsByBuildingIdAsync(int buildingId)
    {
        return await FindAsync(f => f.BuildingId == buildingId);
    }

    public async Task<bool> FloorNumberExistsInBuildingAsync(int floorNumber, int buildingId)
    {
        return await AnyAsync(f => f.BuildingId == buildingId && f.FloorNumber == floorNumber);
    }

    public async Task<Floor?> GetFloorWithDetailsAsync(int id)
    {
        return await _dbContext.Set<Floor>()
            .Include(f => f.Building)
            .Include(f => f.Zones)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
}

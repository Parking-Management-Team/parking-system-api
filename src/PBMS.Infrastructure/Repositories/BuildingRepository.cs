using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai repository cho entity Building.
/// </summary>
public class BuildingRepository : BaseRepository<Building>, IBuildingRepository
{
    private readonly AppDbContext _dbContext;

    public BuildingRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BuildingCodeExistsAsync(string code)
    {
        return await AnyAsync(b => b.Code.ToLower() == code.ToLower());
    }

    public async Task<Building?> GetBuildingWithDetailsAsync(int id)
    {
        return await _dbContext.Set<Building>()
            .Include(b => b.Floors)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<int> GetTotalMotorcycleCapacityAsync(int buildingId)
    {
        return await _dbContext.Set<Zone>()
            .Where(z => z.Floor.BuildingId == buildingId && z.VehicleType.TypeName == VehicleType.MotorcycleTypeName)
            .SumAsync(z => z.Capacity);
    }
}


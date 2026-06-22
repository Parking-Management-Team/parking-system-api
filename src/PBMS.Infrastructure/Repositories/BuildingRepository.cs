using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
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

    /// <summary>
    /// Lấy tổng General Capacity của Building cho một loại xe cụ thể.
    /// Tính bằng tổng capacity của các Zone có AccessType = General và VehicleTypeId khớp.
    /// </summary>
    public async Task<int> GetTotalGeneralCapacityAsync(int buildingId, int vehicleTypeId)
    {
        return await _dbContext.Set<Zone>()
            .Include(z => z.Floor)
            .Where(z =>
                z.Floor.BuildingId == buildingId &&
                z.VehicleTypeId == vehicleTypeId &&
                z.AccessType == ZoneAccessType.General &&
                z.Status == ZoneStatus.Available)
            .SumAsync(z => z.Capacity);
    }
}



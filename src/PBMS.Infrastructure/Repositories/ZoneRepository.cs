using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai repository cho entity Zone.
/// Cung cấp các phương thức truy xuất dữ liệu chuyên biệt cho quản lý zone.
/// </summary>
public class ZoneRepository : BaseRepository<Zone>, IZoneRepository
{
    private readonly AppDbContext _dbContext;

    public ZoneRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy tất cả zone theo floor bất đồng bộ.
    /// </summary>
    public async Task<IEnumerable<Zone>> GetZonesByFloorIdAsync(int floorId)
    {
        return await FindAsync(z => z.FloorId == floorId);
    }

    /// <summary>
    /// Lấy tất cả zone theo loại xe bất đồng bộ.
    /// </summary>
    public async Task<IEnumerable<Zone>> GetZonesByVehicleTypeIdAsync(int vehicleTypeId)
    {
        return await FindAsync(z => z.VehicleTypeId == vehicleTypeId);
    }

    /// <summary>
    /// Kiểm tra tên zone đã tồn tại trong floor cụ thể bất đồng bộ.
    /// </summary>
    public async Task<bool> ZoneNameExistsInFloorAsync(string name, int floorId)
    {
        return await AnyAsync(z => z.FloorId == floorId && z.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Lấy zone kèm floor và parking slots bất đồng bộ.
    /// </summary>
    public async Task<Zone?> GetZoneWithDetailsAsync(int id)
    {
        return await _dbContext.Set<Zone>()
            .Include(z => z.Floor)
            .Include(z => z.VehicleType)
            .Include(z => z.ParkingSlots)
            .FirstOrDefaultAsync(z => z.Id == id);
    }
}

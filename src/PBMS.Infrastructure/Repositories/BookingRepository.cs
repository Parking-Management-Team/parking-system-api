using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;
using BookingEntity = PBMS.Domain.Entities.Booking;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể Đặt chỗ trước (Booking).
/// </summary>
public class BookingRepository : BaseRepository<BookingEntity>, IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy toàn bộ Booking kèm thông tin Account, Vehicle, Building.
    /// </summary>
    public async Task<IEnumerable<BookingEntity>> GetAllWithDetailsAsync()
    {
        return await _dbContext.Set<BookingEntity>()
            .Include(b => b.Account)
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleType)
            .Include(b => b.Building)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy Booking theo ID kèm thông tin liên quan.
    /// </summary>
    public async Task<BookingEntity?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbContext.Set<BookingEntity>()
            .Include(b => b.Account)
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleType)
            .Include(b => b.Building)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Lấy danh sách Booking theo AccountId.
    /// </summary>
    public async Task<IEnumerable<BookingEntity>> GetByAccountIdAsync(int accountId)
    {
        return await _dbContext.Set<BookingEntity>()
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleType)
            .Include(b => b.Building)
            .Where(b => b.AccountId == accountId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách Booking theo BuildingId.
    /// </summary>
    public async Task<IEnumerable<BookingEntity>> GetByBuildingIdAsync(int buildingId)
    {
        return await _dbContext.Set<BookingEntity>()
            .Include(b => b.Account)
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.VehicleType)
            .Where(b => b.BuildingId == buildingId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Đếm số Booking đang chiếm chỗ tại Building cho loại xe trong khoảng thời gian chồng lấn với [start, end].
    /// </summary>
    public async Task<int> GetActiveBookingsCountAsync(int buildingId, int vehicleTypeId, DateTime start, DateTime end)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Set<BookingEntity>()
            .Include(b => b.Vehicle)
            .CountAsync(b =>
                b.BuildingId == buildingId &&
                b.Vehicle.VehicleTypeId == vehicleTypeId &&
                (b.BookingStatus == BookingStatus.Confirmed ||
                 (b.BookingStatus == BookingStatus.Pending && b.PaymentDeadline > now)) &&
                b.PlannedCheckinTime < end &&
                b.PlannedCheckoutTime > start);
    }
}

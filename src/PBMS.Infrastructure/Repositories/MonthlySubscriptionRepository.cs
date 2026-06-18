using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

/// <summary>
/// Triển khai Repository cho thực thể Đăng ký vé tháng (MonthlySubscription).
/// </summary>
public class MonthlySubscriptionRepository : BaseRepository<MonthlySubscription>, IMonthlySubscriptionRepository
{
    public MonthlySubscriptionRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Lấy đăng ký vé tháng đang hoạt động (ACTIVE) của một thẻ gửi xe cụ thể.
    /// </summary>
    public async Task<MonthlySubscription?> GetActiveSubscriptionByCardIdAsync(int cardId)
    {
        return await _dbSet
            .Include(ms => ms.Vehicle)
            .ThenInclude(v => v.VehicleType)
            .FirstOrDefaultAsync(ms => ms.AssignedCardId == cardId && ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active);
    }

    /// <summary>
    /// Lấy đăng ký vé tháng đang hoạt động (ACTIVE) của một xe cụ thể.
    /// </summary>
    public async Task<MonthlySubscription?> GetActiveSubscriptionByVehicleIdAsync(int vehicleId)
    {
        return await _dbSet
            .Include(ms => ms.Vehicle)
            .FirstOrDefaultAsync(ms => ms.VehicleId == vehicleId && ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active);
    }

    /// <summary>
    /// Kiểm tra xe có đăng ký nào đang ACTIVE hoặc PENDING bị chồng lấn hay không.
    /// </summary>
    public async Task<bool> HasOverlapSubscriptionAsync(int vehicleId, int? excludeId = null)
    {
        var query = _dbSet.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(ms => ms.Id != excludeId.Value);
        }

        return await query.AnyAsync(ms => ms.VehicleId == vehicleId && 
            (ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active || 
             ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Pending));
    }

    /// <summary>
    /// Đếm số lượng đăng ký vé tháng xe máy đang ACTIVE hoặc PENDING tại một tòa nhà cụ thể.
    /// Dùng để quản lý sức chứa (capacity) xe máy động.
    /// </summary>
    public async Task<int> GetActiveAndPendingMotorcycleSubscriptionsCountAsync(int buildingId)
    {
        return await _dbSet
            .Include(ms => ms.Vehicle)
            .ThenInclude(v => v.VehicleType)
            .CountAsync(ms => ms.BuildingId == buildingId && 
                (ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active || ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Pending) && 
                ms.Vehicle.VehicleType.TypeName == VehicleType.MotorcycleTypeName);
    }

    /// <summary>
    /// Lấy danh sách các đăng ký tháng ở trạng thái PENDING quá thời gian chờ thanh toán (timeout).
    /// </summary>
    public async Task<IEnumerable<MonthlySubscription>> GetTimeoutPendingSubscriptionsAsync(int timeoutMinutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
        return await _dbSet
            .Where(ms => ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Pending && ms.CreatedAt < cutoff)
            .ToListAsync();
    }
}

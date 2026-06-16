using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class MonthlySubscriptionRepository : BaseRepository<MonthlySubscription>, IMonthlySubscriptionRepository
{
    public MonthlySubscriptionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<MonthlySubscription?> GetActiveSubscriptionByCardCodeAsync(string cardCode)
    {
        var normalizedCardCode = cardCode.Trim().ToUpperInvariant();

        return await _dbSet
            .Include(ms => ms.Vehicle)
            .Include(ms => ms.AssignedCard)
            .FirstOrDefaultAsync(ms => 
                ms.AssignedCard != null && 
                ms.AssignedCard.CardCode == normalizedCardCode &&
                ms.MonthlySubscriptionStatus == "ACTIVE" &&
                ms.ExpiredAt >= DateTime.UtcNow);
    }

    public async Task<MonthlySubscription?> GetActiveSubscriptionByVehicleIdAsync(int vehicleId)
    {
        return await _dbSet
            .Include(ms => ms.Vehicle)
            .Include(ms => ms.AssignedCard)
            .FirstOrDefaultAsync(ms => 
                ms.VehicleId == vehicleId &&
                ms.MonthlySubscriptionStatus == "ACTIVE" &&
                ms.ExpiredAt >= DateTime.UtcNow);
    }

    public async Task<bool> IsCardAssignedToActiveSubscriptionAsync(int cardId)
    {
        return await _dbSet
            .AnyAsync(ms => 
                ms.AssignedCardId == cardId && 
                ms.MonthlySubscriptionStatus == "ACTIVE" &&
                ms.ExpiredAt >= DateTime.UtcNow);
    }
}

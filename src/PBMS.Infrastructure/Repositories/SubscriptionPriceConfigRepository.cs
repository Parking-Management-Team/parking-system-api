using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class SubscriptionPriceConfigRepository : BaseRepository<SubscriptionPriceConfig>, ISubscriptionPriceConfigRepository
{
    public SubscriptionPriceConfigRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SubscriptionPriceConfig?> GetActiveConfigByVehicleTypeAsync(int vehicleTypeId)
    {
        return await _dbSet
            .Where(x => x.VehicleTypeId == vehicleTypeId && x.IsActive && 
                        x.EffectiveFrom <= DateTime.UtcNow && 
                        (!x.EffectiveTo.HasValue || x.EffectiveTo > DateTime.UtcNow))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();
    }
}

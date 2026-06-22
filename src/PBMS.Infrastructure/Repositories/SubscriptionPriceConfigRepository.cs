using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories;

public class SubscriptionPriceConfigRepository : BaseRepository<SubscriptionPriceConfig>, ISubscriptionPriceConfigRepository
{
    public SubscriptionPriceConfigRepository(AppDbContext context, IServiceProvider serviceProvider = null!) 
        : base(context, serviceProvider)
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

    public async Task<IEnumerable<SubscriptionPriceConfig>> GetAllConfigsWithDetailsAsync(int? vehicleTypeId, bool? onlyActive)
    {
        var query = _dbSet.Include(x => x.VehicleType).AsQueryable();

        if (vehicleTypeId.HasValue)
        {
            query = query.Where(x => x.VehicleTypeId == vehicleTypeId.Value);
        }

        if (onlyActive.HasValue && onlyActive.Value)
        {
            query = query.Where(x => x.IsActive && 
                                     x.EffectiveFrom <= DateTime.UtcNow && 
                                     (!x.EffectiveTo.HasValue || x.EffectiveTo > DateTime.UtcNow));
        }

        return await query.OrderByDescending(x => x.EffectiveFrom).ToListAsync();
    }
}

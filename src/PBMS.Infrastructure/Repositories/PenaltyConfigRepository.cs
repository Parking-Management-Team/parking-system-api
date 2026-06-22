using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories;

public class PenaltyConfigRepository : BaseRepository<PenaltyConfig>, IPenaltyConfigRepository
{
    public PenaltyConfigRepository(AppDbContext context, IServiceProvider serviceProvider = null!) 
        : base(context, serviceProvider)
    {
    }

    public async Task<PenaltyConfig?> GetActiveConfigByIncidentTypeAsync(int incidentTypeId)
    {
        return await _dbSet
            .Where(x => x.IncidentTypeId == incidentTypeId && x.IsActive && 
                        x.EffectiveFrom <= DateTime.UtcNow && 
                        (!x.EffectiveTo.HasValue || x.EffectiveTo > DateTime.UtcNow))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PenaltyConfig>> GetAllConfigsWithDetailsAsync(int? incidentTypeId, bool? onlyActive)
    {
        var query = _dbSet.Include(x => x.IncidentType).AsQueryable();

        if (incidentTypeId.HasValue)
        {
            query = query.Where(x => x.IncidentTypeId == incidentTypeId.Value);
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

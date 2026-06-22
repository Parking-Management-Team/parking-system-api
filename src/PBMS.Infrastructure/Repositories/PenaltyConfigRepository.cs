using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class PenaltyConfigRepository : BaseRepository<PenaltyConfig>, IPenaltyConfigRepository
{
    public PenaltyConfigRepository(AppDbContext context) : base(context)
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
}

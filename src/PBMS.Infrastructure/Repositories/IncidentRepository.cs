using Microsoft.EntityFrameworkCore;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class IncidentRepository : BaseRepository<PBMS.Domain.Entities.Incident>, IIncidentRepository
{
    public IncidentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PBMS.Domain.Entities.Incident?> GetIncidentWithDetailsAsync(int id)
    {
        return await _context.Set<PBMS.Domain.Entities.Incident>()
            .Include(i => i.IncidentType)
            .Include(i => i.Session)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<PagedResult<PBMS.Domain.Entities.Incident>> GetPagedIncidentsWithDetailsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Set<PBMS.Domain.Entities.Incident>()
            .Include(i => i.IncidentType)
            .Include(i => i.Session)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PBMS.Domain.Entities.Incident>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PBMS.Domain.Entities.Incident>> GetIncidentsBySessionWithDetailsAsync(int sessionId)
    {
        return await _context.Set<PBMS.Domain.Entities.Incident>()
            .Include(i => i.IncidentType)
            .Where(i => i.SessionId == sessionId)
            .ToListAsync();
    }
}

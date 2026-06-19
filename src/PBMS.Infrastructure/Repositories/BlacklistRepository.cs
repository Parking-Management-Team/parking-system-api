using Microsoft.EntityFrameworkCore;
using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class BlacklistRepository : BaseRepository<PBMS.Domain.Entities.Blacklist>, IBlacklistRepository
{
    public BlacklistRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PBMS.Domain.Entities.Blacklist?> GetBlacklistWithDetailsAsync(int id)
    {
        return await _context.Set<PBMS.Domain.Entities.Blacklist>()
            .Include(b => b.Vehicle)
            .Include(b => b.Card)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PagedResult<PBMS.Domain.Entities.Blacklist>> GetPagedBlacklistWithDetailsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Set<PBMS.Domain.Entities.Blacklist>()
            .Include(b => b.Vehicle)
            .Include(b => b.Card)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PBMS.Domain.Entities.Blacklist>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}

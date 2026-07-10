using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using ShiftReportEntity = PBMS.Domain.Entities.ShiftReport;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class ShiftReportRepository : BaseRepository<ShiftReportEntity>, IShiftReportRepository
{
    public ShiftReportRepository(AppDbContext context, IServiceProvider serviceProvider = null!) 
        : base(context, serviceProvider)
    {
    }

    public async Task<ShiftReportEntity?> GetLastReportByStaffIdAsync(int staffId)
    {
        return await _dbSet
            .Where(sr => sr.StaffId == staffId && !sr.IsDeleted)
            .OrderByDescending(sr => sr.EndTime)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<ShiftReportEntity> Items, int TotalCount)> GetPagedWithDetailsAsync(int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Include(sr => sr.Staff)
            .Include(sr => sr.ApprovedBy)
            .Where(sr => !sr.IsDeleted);

        int totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(sr => sr.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Payment entity.
    /// </summary>
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        private readonly AppDbContext _dbContext;

        public PaymentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a paginated list of payments with optional filters.
        /// </summary>
        public async Task<(IEnumerable<Payment> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? method)
        {
            var query = _dbContext.Set<Payment>()
                .Include(p => p.Session)
                .Include(p => p.Booking)
                .Include(p => p.MonthlySubscription)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                var upperMethod = method.ToUpper();
                query = query.Where(p => p.PaymentMethod.ToUpper() == upperMethod);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Retrieves all payments associated with a parking session.
        /// </summary>
        public async Task<IEnumerable<Payment>> GetBySessionIdAsync(int sessionId)
        {
            return await _dbContext.Set<Payment>()
                .Include(p => p.Session)
                .Where(p => p.SessionId == sessionId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all payments associated with an account.
        /// </summary>
        public async Task<IEnumerable<Payment>> GetByAccountIdAsync(int accountId)
        {
            return await _dbContext.Set<Payment>()
                .Include(p => p.Session)
                    .ThenInclude(s => s!.Vehicle)
                .Include(p => p.Booking)
                .Include(p => p.MonthlySubscription)
                .Where(p => 
                    (p.Session != null && p.Session.Vehicle != null && p.Session.Vehicle.AccountId == accountId) ||
                    (p.Booking != null && p.Booking.AccountId == accountId) ||
                    (p.MonthlySubscription != null && p.MonthlySubscription.AccountId == accountId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}

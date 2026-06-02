using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Infrastructure.Data;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IAccountRepository for EF Core operations on Account entities.
    /// </summary>
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves an account by its email, including its associated Role.
        /// </summary>
        /// <param name="email">The email of the account.</param>
        /// <returns>The account if found; otherwise, null.</returns>
        public async Task<Account?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();

            return await _dbSet
                .Include(a => a.Role) // Eagerly load Role for JWT Claims construction
                .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail);
        }
    }
}

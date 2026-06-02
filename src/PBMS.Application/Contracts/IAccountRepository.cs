using System.Threading.Tasks;
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts
{
    /// <summary>
    /// Contract for account-specific database operations.
    /// Extends the generic repository contract for the Account entity.
    /// </summary>
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>
        /// Retrieves an account by its unique email address.
        /// </summary>
        /// <param name="email">The email of the account to retrieve.</param>
        /// <returns>The account if found; otherwise, null.</returns>
        Task<Account?> GetByEmailAsync(string email);
    }
}

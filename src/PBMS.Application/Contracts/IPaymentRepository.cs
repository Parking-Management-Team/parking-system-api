using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentEntity = PBMS.Domain.Entities.Payment;

namespace PBMS.Application.Contracts
{
    /// <summary>
    /// Repository interface for Payment entity.
    /// </summary>
    public interface IPaymentRepository : IRepository<PaymentEntity>
    {
        /// <summary>
        /// Retrieves a paginated list of payments with optional filters.
        /// </summary>
        Task<(IEnumerable<PaymentEntity> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? method);

        /// <summary>
        /// Retrieves all payments associated with a parking session.
        /// </summary>
        Task<IEnumerable<PaymentEntity>> GetBySessionIdAsync(int sessionId);

        /// <summary>
        /// Retrieves all payments associated with an account.
        /// </summary>
        Task<IEnumerable<PaymentEntity>> GetByAccountIdAsync(int accountId);
    }
}

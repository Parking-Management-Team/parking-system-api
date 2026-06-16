using PBMS.Application.MonthlyCard.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBMS.Application.MonthlyCard.Interfaces;

public interface IMonthlySubscriptionService
{
    Task<MonthlySubscriptionDto> RegisterSubscriptionAsync(CreateMonthlySubscriptionRequest request);
    Task<MonthlySubscriptionDto> ActivateSubscriptionAsync(int subscriptionId, string cardCode);
    Task<MonthlySubscriptionDto> RenewSubscriptionAsync(int subscriptionId, int months);
    Task<MonthlySubscriptionDto> ReportLostCardAsync(int subscriptionId);
    Task<MonthlySubscriptionDto> ReassignCardAsync(int subscriptionId, string newCardCode);
    Task<MonthlySubscriptionDto> GetByIdAsync(int id);
    Task<IEnumerable<MonthlySubscriptionDto>> GetAllAsync();
}

using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface ISubscriptionPriceConfigRepository : IRepository<SubscriptionPriceConfig>
{
    Task<SubscriptionPriceConfig?> GetActiveConfigByVehicleTypeAsync(int vehicleTypeId);
}

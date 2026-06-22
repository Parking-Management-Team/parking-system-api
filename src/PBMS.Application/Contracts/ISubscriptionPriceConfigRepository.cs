using System.Collections.Generic;
using System.Threading.Tasks;
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface ISubscriptionPriceConfigRepository : IRepository<SubscriptionPriceConfig>
{
    Task<SubscriptionPriceConfig?> GetActiveConfigByVehicleTypeAsync(int vehicleTypeId);
    Task<IEnumerable<SubscriptionPriceConfig>> GetAllConfigsWithDetailsAsync(int? vehicleTypeId, bool? onlyActive);
}

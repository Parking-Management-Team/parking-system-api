using System.Collections.Generic;
using System.Threading.Tasks;
using PBMS.Application.Pricing.DTOs;

namespace PBMS.Application.Pricing.Interfaces;

public interface ISubscriptionPriceConfigService
{
    Task<IEnumerable<SubscriptionPriceConfigDto>> GetAllConfigsAsync(int? vehicleTypeId, bool? onlyActive);
    Task<SubscriptionPriceConfigDto?> GetActiveConfigByVehicleTypeAsync(int vehicleTypeId);
    Task<SubscriptionPriceConfigDto> CreateConfigAsync(CreateSubscriptionPriceConfigRequest request);
    Task<bool> DeactivateConfigAsync(int id);
    Task<bool> DeleteConfigAsync(int id);
}

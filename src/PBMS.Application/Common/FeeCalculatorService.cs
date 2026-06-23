using PBMS.Application.Contracts;

namespace PBMS.Application.Common;

public class FeeCalculatorService : IFeeCalculatorService
{
    private readonly IPenaltyConfigRepository _penaltyConfigRepository;
    private readonly ISubscriptionPriceConfigRepository _subscriptionPriceConfigRepository;

    public FeeCalculatorService(
        IPenaltyConfigRepository penaltyConfigRepository,
        ISubscriptionPriceConfigRepository subscriptionPriceConfigRepository)
    {
        _penaltyConfigRepository = penaltyConfigRepository;
        _subscriptionPriceConfigRepository = subscriptionPriceConfigRepository;
    }

    public async Task<decimal> CalculatePenaltyFeeAsync(int incidentTypeId)
    {
        var config = await _penaltyConfigRepository.GetActiveConfigByIncidentTypeAsync(incidentTypeId);
        if (config != null)
        {
            return config.PenaltyFee;
        }

        // Fallback to 0 if no active config found
        return 0;
    }

    public async Task<decimal> CalculateSubscriptionFeeAsync(int vehicleTypeId)
    {
        var config = await _subscriptionPriceConfigRepository.GetActiveConfigByVehicleTypeAsync(vehicleTypeId);
        if (config != null)
        {
            return config.Price;
        }

        // Fallback or throw exception? Let's fallback to 0 or throw
        throw new InvalidOperationException($"No active subscription price configuration found for VehicleTypeId: {vehicleTypeId}");
    }
}

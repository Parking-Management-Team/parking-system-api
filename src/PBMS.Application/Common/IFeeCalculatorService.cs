namespace PBMS.Application.Common;

public interface IFeeCalculatorService
{
    Task<decimal> CalculatePenaltyFeeAsync(int incidentTypeId);
    Task<decimal> CalculateSubscriptionFeeAsync(int vehicleTypeId);
}

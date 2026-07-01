using System;
using System.Text.Json;
using System.Threading.Tasks;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Engine;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Pricing.Services;

/// <summary>
/// Dịch vụ tính toán phí đỗ xe triển khai Pricing Engine và ghi log audit.
/// </summary>
public class PricingCalculationService : IPricingCalculationService
{
    private readonly IPricingPolicyRepository _pricingPolicyRepository;
    private readonly IPricingEngine _pricingEngine;
    private readonly IRepository<PricingCalculationLog> _logRepository;

    public PricingCalculationService(
        IPricingPolicyRepository pricingPolicyRepository,
        IPricingEngine pricingEngine,
        IRepository<PricingCalculationLog> logRepository)
    {
        _pricingPolicyRepository = pricingPolicyRepository ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
    }

    public async Task<PricingResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut)
    {
        var policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá đang hoạt động cho loại phương tiện ID = {vehicleTypeId} tại thời điểm check-in: {checkIn:dd/MM/yyyy HH:mm:ss}."
            );
        }

        return _pricingEngine.Calculate(policy, checkIn, checkOut);
    }

    public async Task<PricingResult> CalculateFeeAndLogAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut,
        int? bookingId = null,
        int? parkingSessionId = null)
    {
        var policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá đang hoạt động cho loại phương tiện ID = {vehicleTypeId} tại thời điểm check-in: {checkIn:dd/MM/yyyy HH:mm:ss}."
            );
        }

        var result = _pricingEngine.Calculate(policy, checkIn, checkOut);

        // Ghi log audit tính phí
        var log = new PricingCalculationLog
        {
            BookingId = bookingId,
            ParkingSessionId = parkingSessionId,
            VehicleTypeId = vehicleTypeId,
            CheckInTime = checkIn,
            CheckOutTime = checkOut,
            MatchedPolicyId = policy.Id,
            TotalPrice = result.TotalAmount,
            CalculationDetails = JsonSerializer.Serialize(result.RuleResults)
        };

        await _logRepository.AddAsync(log);
        await _logRepository.SaveChangesAsync();

        return result;
    }
}

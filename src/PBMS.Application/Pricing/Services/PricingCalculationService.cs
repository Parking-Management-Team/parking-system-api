using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSystemConfig.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Engine;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Pricing.Services;

/// <summary>
/// Parking fee calculation service that uses the Pricing Engine and writes audit logs.
/// Supports segmented calculations based on the APPLY_SEGMENTED_PRICING config switch.
/// </summary>
public class PricingCalculationService : IPricingCalculationService
{
    private readonly IPricingPolicyRepository _pricingPolicyRepository;
    private readonly IPricingEngine _pricingEngine;
    private readonly IRepository<PricingCalculationLog> _logRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IPenaltyConfigRepository _penaltyConfigRepository;
    private readonly IParkingSystemConfigService _configService;

    public PricingCalculationService(
        IPricingPolicyRepository pricingPolicyRepository,
        IPricingEngine pricingEngine,
        IRepository<PricingCalculationLog> logRepository,
        IIncidentRepository incidentRepository,
        IPenaltyConfigRepository penaltyConfigRepository,
        IParkingSystemConfigService configService)
    {
        _pricingPolicyRepository = pricingPolicyRepository ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _penaltyConfigRepository = penaltyConfigRepository ?? throw new ArgumentNullException(nameof(penaltyConfigRepository));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    public Task<PricingResult> CalculatePreviewAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? parkingSessionId = null)
    {
        return CalculateFeeAsync(vehicleTypeId, checkIn, checkOut, parkingSessionId);
    }

    public async Task<PricingResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? parkingSessionId = null)
    {
        var config = await _configService.GetByKeyAsync("APPLY_SEGMENTED_PRICING");
        bool applySegmented = config != null && string.Equals(config.Value, "true", StringComparison.OrdinalIgnoreCase);

        PricingPolicy? policy = null;
        Func<DateTime, PricingPolicy>? getPolicyAtTime = null;

        if (applySegmented)
        {
            var policies = await _pricingPolicyRepository.GetPoliciesInPeriodAsync(vehicleTypeId, checkIn, checkOut);
            if (!policies.Any())
            {
                throw new DomainException(
                    errorCode: "PRICING_POLICY_NOT_FOUND",
                    message: $"No active pricing policy was found for vehicle type ID {vehicleTypeId} in the period {checkIn:dd/MM/yyyy HH:mm:ss} to {checkOut:dd/MM/yyyy HH:mm:ss}."
                );
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            getPolicyAtTime = (t) =>
            {
                var localTime = t.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(t, tz) : t;
                var localDate = localTime.Date;

                var matched = policies
                    .Where(pp => pp.EffectiveStart <= localDate && (pp.EffectiveEnd == null || pp.EffectiveEnd.Value >= localDate))
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveStart)
                    .ThenByDescending(pp => pp.Id)
                    .FirstOrDefault();

                if (matched == null)
                {
                    matched = policies.OrderBy(pp => Math.Abs((pp.EffectiveStart - localDate).Ticks)).First();
                }

                return matched;
            };
        }
        else
        {
            policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);
            if (policy == null)
            {
                throw new DomainException(
                    errorCode: "PRICING_POLICY_NOT_FOUND",
                    message: $"No active pricing policy was found for vehicle type ID {vehicleTypeId} at check-in time {checkIn:dd/MM/yyyy HH:mm:ss}."
                );
            }
        }

        IEnumerable<PBMS.Domain.Entities.Incident>? incidents = null;
        IEnumerable<PenaltyConfig>? penaltyConfigs = null;

        if (parkingSessionId.HasValue)
        {
            var allIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(parkingSessionId.Value);
            incidents = allIncidents.Where(i => (i.Status == PBMS.Domain.Enums.IncidentStatus.Open || i.Status == PBMS.Domain.Enums.IncidentStatus.Processing) && !i.IsDeleted).ToList();

            if (incidents.Any())
            {
                penaltyConfigs = await _penaltyConfigRepository.GetAllConfigsWithDetailsAsync(incidentTypeId: null, onlyActive: true);
            }
        }

        if (applySegmented && getPolicyAtTime != null)
        {
            return _pricingEngine.CalculateSegmented(getPolicyAtTime, checkIn, checkOut, incidents, penaltyConfigs);
        }
        else
        {
            return _pricingEngine.Calculate(policy!, checkIn, checkOut, incidents, penaltyConfigs);
        }
    }

    public async Task<PricingResult> CalculateCommittedFeeAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut,
        string calculationPurpose,
        int? bookingId = null,
        int? parkingSessionId = null,
        int? paymentId = null,
        string? idempotencyKey = null)
    {
        // 1. Idempotency Check: nếu đã có log với IdempotencyKey này, trả về kết quả đã tính từ trước mà không ghi đè log
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existingLogs = await _logRepository.FindAsync(l => l.IdempotencyKey == idempotencyKey);
            var existingLog = existingLogs.FirstOrDefault();
            if (existingLog != null)
            {
                return new PricingResult
                {
                    TotalAmount = existingLog.TotalPrice,
                    RuleResults = !string.IsNullOrEmpty(existingLog.CalculationDetails)
                        ? JsonSerializer.Deserialize<List<RuleResult>>(existingLog.CalculationDetails) ?? new List<RuleResult>()
                        : new List<RuleResult>()
                };
            }
        }

        var config = await _configService.GetByKeyAsync("APPLY_SEGMENTED_PRICING");
        bool applySegmented = config != null && string.Equals(config.Value, "true", StringComparison.OrdinalIgnoreCase);

        PricingPolicy? policy = null;
        Func<DateTime, PricingPolicy>? getPolicyAtTime = null;

        if (applySegmented)
        {
            var policies = await _pricingPolicyRepository.GetPoliciesInPeriodAsync(vehicleTypeId, checkIn, checkOut);
            if (!policies.Any())
            {
                throw new DomainException(
                    errorCode: "PRICING_POLICY_NOT_FOUND",
                    message: $"No active pricing policy was found for vehicle type ID {vehicleTypeId} in the period {checkIn:dd/MM/yyyy HH:mm:ss} to {checkOut:dd/MM/yyyy HH:mm:ss}."
                );
            }

            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            getPolicyAtTime = (t) =>
            {
                var localTime = t.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(t, tz) : t;
                var localDate = localTime.Date;

                var matched = policies
                    .Where(pp => pp.EffectiveStart <= localDate && (pp.EffectiveEnd == null || pp.EffectiveEnd.Value >= localDate))
                    .OrderByDescending(pp => pp.Priority)
                    .ThenByDescending(pp => pp.EffectiveStart)
                    .ThenByDescending(pp => pp.Id)
                    .FirstOrDefault();

                if (matched == null)
                {
                    matched = policies.OrderBy(pp => Math.Abs((pp.EffectiveStart - localDate).Ticks)).First();
                }

                return matched;
            };
        }
        else
        {
            policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);
            if (policy == null)
            {
                throw new DomainException(
                    errorCode: "PRICING_POLICY_NOT_FOUND",
                    message: $"No active pricing policy was found for vehicle type ID {vehicleTypeId} at check-in time {checkIn:dd/MM/yyyy HH:mm:ss}."
                );
            }
        }

        IEnumerable<PBMS.Domain.Entities.Incident>? incidents = null;
        IEnumerable<PenaltyConfig>? penaltyConfigs = null;

        if (parkingSessionId.HasValue)
        {
            var allIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(parkingSessionId.Value);
            incidents = allIncidents.Where(i => (i.Status == PBMS.Domain.Enums.IncidentStatus.Open || i.Status == PBMS.Domain.Enums.IncidentStatus.Processing) && !i.IsDeleted).ToList();

            if (incidents.Any())
            {
                penaltyConfigs = await _penaltyConfigRepository.GetAllConfigsWithDetailsAsync(incidentTypeId: null, onlyActive: true);
            }
        }

        PricingResult result;
        int matchedPolicyId;

        if (applySegmented && getPolicyAtTime != null)
        {
            result = _pricingEngine.CalculateSegmented(getPolicyAtTime, checkIn, checkOut, incidents, penaltyConfigs);
            matchedPolicyId = getPolicyAtTime(checkOut.AddSeconds(-1)).Id;
        }
        else
        {
            result = _pricingEngine.Calculate(policy!, checkIn, checkOut, incidents, penaltyConfigs);
            matchedPolicyId = policy!.Id;
        }

        // 2. Ghi bản ghi PricingCalculationLog đối soát tài chính chính thức
        var log = new PricingCalculationLog
        {
            BookingId = bookingId,
            ParkingSessionId = parkingSessionId,
            PaymentId = paymentId,
            CalculationPurpose = string.IsNullOrWhiteSpace(calculationPurpose) ? "CHECKOUT_FINAL" : calculationPurpose,
            IdempotencyKey = idempotencyKey,
            VehicleTypeId = vehicleTypeId,
            CheckInTime = checkIn,
            CheckOutTime = checkOut,
            MatchedPolicyId = matchedPolicyId,
            TotalPrice = result.TotalAmount,
            CalculationDetails = JsonSerializer.Serialize(result.RuleResults)
        };

        await _logRepository.AddAsync(log);
        await _logRepository.SaveChangesAsync();

        return result;
    }

    public Task<PricingResult> CalculateFeeAndLogAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut,
        int? bookingId = null,
        int? parkingSessionId = null,
        string calculationPurpose = "CHECKOUT_FINAL",
        int? paymentId = null,
        string? idempotencyKey = null)
    {
        return CalculateCommittedFeeAsync(
            vehicleTypeId,
            checkIn,
            checkOut,
            calculationPurpose,
            bookingId,
            parkingSessionId,
            paymentId,
            idempotencyKey);
    }
}

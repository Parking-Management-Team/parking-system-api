using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IIncidentRepository _incidentRepository;
    private readonly IPenaltyConfigRepository _penaltyConfigRepository;

    public PricingCalculationService(
        IPricingPolicyRepository pricingPolicyRepository,
        IPricingEngine pricingEngine,
        IRepository<PricingCalculationLog> logRepository,
        IIncidentRepository incidentRepository,
        IPenaltyConfigRepository penaltyConfigRepository)
    {
        _pricingPolicyRepository = pricingPolicyRepository ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _penaltyConfigRepository = penaltyConfigRepository ?? throw new ArgumentNullException(nameof(penaltyConfigRepository));
    }

    public async Task<PricingResult> CalculateFeeAsync(int vehicleTypeId, DateTime checkIn, DateTime checkOut, int? parkingSessionId = null)
    {
        var policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);
        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"Không tìm thấy chính sách giá đang hoạt động cho loại phương tiện ID = {vehicleTypeId} tại thời điểm check-in: {checkIn:dd/MM/yyyy HH:mm:ss}."
            );
        }

        IEnumerable<PBMS.Domain.Entities.Incident>? incidents = null;
        IEnumerable<PenaltyConfig>? penaltyConfigs = null;

        if (parkingSessionId.HasValue)
        {
            // Lấy danh sách các sự cố chưa được xử lý (hoặc đang Open) của session
            var allIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(parkingSessionId.Value);
            incidents = allIncidents.Where(i => i.Status == PBMS.Domain.Enums.IncidentStatus.Open && !i.IsDeleted).ToList();

            if (incidents.Any())
            {
                // Lấy tất cả cấu hình giá phạt đang hoạt động để làm dữ liệu map cho Engine
                penaltyConfigs = await _penaltyConfigRepository.GetAllConfigsWithDetailsAsync(incidentTypeId: null, onlyActive: true);
            }
        }

        return _pricingEngine.Calculate(policy, checkIn, checkOut, incidents, penaltyConfigs);
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

        IEnumerable<PBMS.Domain.Entities.Incident>? incidents = null;
        IEnumerable<PenaltyConfig>? penaltyConfigs = null;

        if (parkingSessionId.HasValue)
        {
            var allIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(parkingSessionId.Value);
            incidents = allIncidents.Where(i => i.Status == PBMS.Domain.Enums.IncidentStatus.Open && !i.IsDeleted).ToList();

            if (incidents.Any())
            {
                penaltyConfigs = await _penaltyConfigRepository.GetAllConfigsWithDetailsAsync(incidentTypeId: null, onlyActive: true);
            }
        }

        var result = _pricingEngine.Calculate(policy, checkIn, checkOut, incidents, penaltyConfigs);

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

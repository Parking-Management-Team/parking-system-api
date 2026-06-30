using System;
using System.Linq;
using System.Threading.Tasks;
using PBMS.Application.Common;
using PBMS.Application.Common.DTOs;
using PBMS.Application.Common.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using BookingEntity = PBMS.Domain.Entities.Booking;
using IncidentEntity = PBMS.Domain.Entities.Incident;
using ParkingSlotEntity = PBMS.Domain.Entities.ParkingSlot;

namespace PBMS.Application.Common.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<ParkingSessionEntity> _sessionRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IMonthlySubscriptionRepository _subscriptionRepository;
    private readonly IRepository<ParkingSlotEntity> _slotRepository;

    public DashboardService(
        IRepository<ParkingSessionEntity> sessionRepository,
        IBookingRepository bookingRepository,
        IIncidentRepository incidentRepository,
        IMonthlySubscriptionRepository subscriptionRepository,
        IRepository<ParkingSlotEntity> slotRepository)
    {
        _sessionRepository = sessionRepository;
        _bookingRepository = bookingRepository;
        _incidentRepository = incidentRepository;
        _subscriptionRepository = subscriptionRepository;
        _slotRepository = slotRepository;
    }

    public async Task<BaseResponse<DashboardSummaryDto>> GetSummaryAsync()
    {
        var activeSessions = await _sessionRepository.CountAsync(s => s.SessionStatus == "ACTIVE");
        
        var today = DateTime.UtcNow.Date;
        var endOfToday = today.AddDays(1);
        var expectedBookings = await _bookingRepository.CountAsync(b => 
            b.PlannedCheckinTime >= today && 
            b.PlannedCheckinTime < endOfToday && 
            (b.BookingStatus == "Confirmed" || b.BookingStatus == "Pending"));

        var activeIncidents = await _incidentRepository.CountAsync(i => i.Status == IncidentStatus.Open);
        
        var activeSubscriptions = await _subscriptionRepository.CountAsync(s => s.MonthlySubscriptionStatus == "Active");

        var totalSlots = await _slotRepository.CountAsync(s => s.Status != SlotStatus.Maintenance);
        double occupancyRate = 0;
        if (totalSlots > 0)
        {
            var occupiedSlots = await _slotRepository.CountAsync(s => s.Status == SlotStatus.Occupied);
            occupancyRate = Math.Round((double)occupiedSlots / totalSlots * 100, 2);
        }

        var dto = new DashboardSummaryDto
        {
            TotalActiveSessions = activeSessions,
            ExpectedBookingsToday = expectedBookings,
            ActiveIncidentsCount = activeIncidents,
            ActiveMonthlySubscriptions = activeSubscriptions,
            OccupancyRate = occupancyRate
        };

        return BaseResponse<DashboardSummaryDto>.Ok(dto, "Dashboard summary retrieved successfully.");
    }
}

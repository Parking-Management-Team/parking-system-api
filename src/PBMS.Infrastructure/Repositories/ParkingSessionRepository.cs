using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;
using PBMS.Application.ParkingSession.DTOs;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;

namespace PBMS.Infrastructure.Repositories;

public class ParkingSessionRepository : BaseRepository<ParkingSessionEntity>, IParkingSessionRepository
{
    public ParkingSessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate)
    {
        var normalized = PBMS.Application.Vehicle.Services.VehicleService.NormalizeLicensePlate(licensePlate);

        return await _context.Vehicles
            .Include(v => v.VehicleType)
            .FirstOrDefaultAsync(v => v.LicensePlate == normalized);
    }

    public async Task<bool> HasActiveSessionForVehicleAsync(int vehicleId)
    {
        return await _context.ParkingSessions
            .AnyAsync(ps => ps.VehicleId == vehicleId && ps.SessionStatus.ToUpper() == "ACTIVE");
    }

    public async Task<bool> HasActiveSessionForSlotAsync(int slotId)
    {
        return await _context.ParkingSessions
            .AnyAsync(ps => ps.SlotId == slotId && ps.SessionStatus.ToUpper() == "ACTIVE");
    }

    public async Task<bool> HasParkingSessionForBookingAsync(int bookingId)
    {
        return await _context.ParkingSessions
            .AnyAsync(ps => ps.BookingId == bookingId);
    }

    public async Task<Booking?> GetBookingForCheckInAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Vehicle)
            .Include(b => b.VehicleType)
            .Include(b => b.Building)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<Booking?> GetActiveBookingForCheckInByLicensePlateAsync(string licensePlate, int? buildingId = null)
    {
        var normalized = PBMS.Application.Vehicle.Services.VehicleService.NormalizeLicensePlate(licensePlate);
        var now = DateTime.UtcNow;

        var query = _context.Bookings
            .Include(b => b.Vehicle)
            .Include(b => b.VehicleType)
            .Include(b => b.Building)
            .Where(b =>
                b.Vehicle.LicensePlate.ToUpper() == normalized &&
                (b.BookingStatus == BookingStatus.Confirmed || (b.BookingStatus == BookingStatus.Pending && b.PaymentDeadline >= now)) &&
                b.CheckinGraceUntil >= now &&
                !_context.ParkingSessions.Any(ps => ps.BookingId == b.Id));

        if (buildingId.HasValue)
        {
            query = query.Where(b => b.BuildingId == buildingId.Value);
        }

        return await query
            .OrderBy(b => b.PlannedCheckinTime)
            .ThenBy(b => b.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<MonthlySubscription?> GetMonthlySubscriptionForCheckInAsync(int monthlySubscriptionId)
    {
        return await _context.MonthlySubscriptions
            .Include(ms => ms.Vehicle)
            .Include(ms => ms.AssignedCard)
            .Include(ms => ms.AssignedSlot)
            .ThenInclude(s => s!.Zone)
            .ThenInclude(z => z.Floor)
            .FirstOrDefaultAsync(ms => ms.Id == monthlySubscriptionId);
    }

    public async Task<Zone?> FindAvailableZoneAsync(int vehicleTypeId, int? buildingId = null)
    {
        var now = DateTime.UtcNow;
        var startGrace = now.AddMinutes(30);

        var zones = _context.Zones
            .Include(z => z.Floor)
            .Where(z =>
                z.VehicleTypeId == vehicleTypeId &&
                z.AccessType == ZoneAccessType.General &&
                z.Status == ZoneStatus.Available &&
                z.Floor.Status == FloorStatus.Active);

        if (buildingId.HasValue)
        {
            zones = zones.Where(z => z.Floor.BuildingId == buildingId.Value);
        }

        return await zones
            .Select(z => new
            {
                Zone = z,
                ActiveSessions = _context.ParkingSessions.Count(ps =>
                    ps.ZoneId == z.Id &&
                    ps.SessionStatus.ToUpper() == "ACTIVE"),
                ReservedBookings = _context.Set<Booking>().Count(b =>
                    b.SlotId != null &&
                    b.ParkingSlot!.ZoneId == z.Id &&
                    b.BookingStatus == BookingStatus.Confirmed &&
                    b.PlannedCheckinTime <= startGrace &&
                    b.PlannedCheckoutTime > now)
            })
            .Where(x => (x.ActiveSessions + x.ReservedBookings) < x.Zone.Capacity)
            .OrderBy(x => x.ActiveSessions + x.ReservedBookings)
            .ThenBy(x => x.Zone.Id)
            .Select(x => x.Zone)
            .FirstOrDefaultAsync();
    }

    public async Task<ParkingSlot?> FindAvailableGeneralSlotAsync(int vehicleTypeId, int? buildingId = null)
    {
        var now = DateTime.UtcNow;
        var startGrace = now.AddMinutes(30);

        var query = _context.ParkingSlots
            .Include(s => s.Zone)
            .ThenInclude(z => z.Floor)
            .Where(s =>
                s.VehicleTypeId == vehicleTypeId &&
                s.Status == SlotStatus.Available &&
                s.Zone.Status == ZoneStatus.Available &&
                s.Zone.AccessType == ZoneAccessType.General &&
                s.Zone.Floor.Status == FloorStatus.Active);

        if (buildingId.HasValue)
        {
            query = query.Where(s => s.Zone.Floor.BuildingId == buildingId.Value);
        }

        // Keep the existing allocation rules, but let PostgreSQL filter and rank in one round-trip.
        return await query
            .Where(s => !_context.Set<Booking>().Any(b =>
                b.SlotId == s.Id &&
                b.BookingStatus == BookingStatus.Confirmed &&
                b.PlannedCheckinTime <= startGrace &&
                b.PlannedCheckoutTime > now))
            .Select(s => new
            {
                Slot = s,
                BookingCount = _context.Set<Booking>().Count(b =>
                    b.SlotId == s.Id &&
                    (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending) &&
                    b.PlannedCheckinTime > now),
                NextCheckin = _context.Set<Booking>()
                    .Where(b =>
                        b.SlotId == s.Id &&
                        (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Pending) &&
                        b.PlannedCheckinTime > now)
                    .Min(b => (DateTime?)b.PlannedCheckinTime)
            })
            .OrderBy(x => x.BookingCount)
            .ThenByDescending(x => x.NextCheckin == null)
            .ThenByDescending(x => x.NextCheckin)
            .Select(x => x.Slot)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ParkingSlot>> FindAllAvailableGeneralSlotsAsync(int vehicleTypeId, int? buildingId = null)
    {
        var now = DateTime.UtcNow;
        var startGrace = now.AddMinutes(30);

        var query = _context.ParkingSlots
            .Include(s => s.Zone)
            .ThenInclude(z => z.Floor)
            .Where(s =>
                s.VehicleTypeId == vehicleTypeId &&
                s.Status == SlotStatus.Available &&
                s.Zone.Status == ZoneStatus.Available &&
                s.Zone.AccessType == ZoneAccessType.General &&
                s.Zone.Floor.Status == FloorStatus.Active);

        if (buildingId.HasValue)
        {
            query = query.Where(s => s.Zone.Floor.BuildingId == buildingId.Value);
        }

        return await query
            .Where(s => !_context.Set<Booking>().Any(b =>
                b.SlotId == s.Id &&
                b.BookingStatus == BookingStatus.Confirmed &&
                b.PlannedCheckinTime <= startGrace &&
                b.PlannedCheckoutTime > now))
            .ToListAsync();
    }

    public async Task<ParkingSessionEntity?> GetSessionWithDetailsAsync(int id)
    {
        return await _context.ParkingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.Building)
            .Include(s => s.Booking)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> HasPaidPaymentForSessionAsync(int sessionId)
    {
        return await _context.Payments
            .AnyAsync(p => p.SessionId == sessionId && p.PaymentStatus == "PAID");
    }

    public async Task<IEnumerable<ParkingSessionEntity>> GetOvertimeWarningSessionsAsync(DateTime warningTimeLimit, DateTime now)
    {
        return await _context.ParkingSessions
            .Include(s => s.Booking)
            .Include(s => s.Vehicle)
            .Where(s => s.SessionStatus == "ACTIVE" &&
                        s.BookingId != null &&
                        s.Booking!.PlannedCheckoutTime <= warningTimeLimit &&
                        s.Booking!.PlannedCheckoutTime > now)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingSessionEntity>> GetByAccountIdAsync(int accountId)
    {
        return await _context.ParkingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.Card)
            .Include(s => s.Zone)
            .Include(s => s.ParkingSlot)
            .Include(s => s.Payments)
            .Include(s => s.Booking)
                .ThenInclude(b => b!.Payments)
            .Where(s => s.Vehicle.AccountId == accountId)
            .OrderByDescending(s => s.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingSessionEntity>> GetActiveSessionsWithDetailsAsync()
    {
        return await _context.ParkingSessions
            .AsNoTracking()
            .Include(s => s.Vehicle)
                .ThenInclude(v => v.VehicleType)
            .Include(s => s.Card)
            .Include(s => s.Zone)
            .Include(s => s.ParkingSlot)
            .Where(s => s.SessionStatus.ToUpper() == "ACTIVE")
            .ToListAsync();
    }

    public async Task<IEnumerable<ActiveParkingSessionSummaryDto>> GetActiveSessionSummariesAsync()
    {
        return await _context.ParkingSessions
            .AsNoTracking()
            .Where(s => s.SessionStatus.ToUpper() == "ACTIVE")
            .OrderByDescending(s => s.CheckInTime)
            .Select(s => new ActiveParkingSessionSummaryDto
            {
                Id = s.Id,
                VehicleId = s.VehicleId,
                AccountId = s.Vehicle.AccountId,
                BuildingId = s.BuildingId,
                CardId = s.CardId,
                ZoneId = s.ZoneId,
                SlotId = s.SlotId,
                BookingId = s.BookingId,
                MonthlySubscriptionId = null,
                InStaffId = s.InStaffId,
                OutStaffId = s.OutStaffId,
                CheckInTime = s.CheckInTime,
                CheckOutTime = s.CheckOutTime,
                LicensePlateIn = s.LicensePlateIn,
                LicensePlateOut = s.LicensePlateOut,
                SessionStatus = s.SessionStatus,
                CardCode = s.Card.CardCode,
                ZoneCode = s.Zone != null ? s.Zone.Code : null,
                SlotCode = s.ParkingSlot != null ? s.ParkingSlot.Code : null,
                VehicleType = s.Vehicle.VehicleType.TypeName,
                CustomerType = s.BookingId.HasValue ? "BOOKING" : "WALK_IN",
                PricingVehicleTypeId = s.Vehicle.VehicleTypeId,
                BookingPlannedCheckoutTime = s.BookingId.HasValue
                    ? s.Booking!.PlannedCheckoutTime
                    : null,
                TotalFee =
                    s.Payments.Where(p => p.PaymentStatus.ToUpper() == "PAID").Sum(p => (decimal?)p.Amount) +
                    (s.BookingId.HasValue
                        ? s.Booking!.Payments.Where(p => p.PaymentStatus.ToUpper() == "PAID").Sum(p => (decimal?)p.Amount)
                        : 0),
                ImageIn = null,
                ImageOut = null
            })
            .ToListAsync();
    }

    public async Task<ParkingSessionEntity?> FindActiveSessionForSlotAsync(int slotId)
    {
        return await _context.ParkingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.Booking)
            .FirstOrDefaultAsync(s => s.SlotId == slotId && s.SessionStatus.ToUpper() == "ACTIVE");
    }

    public async Task<bool> HasPaidPaymentForBookingAsync(int bookingId)
    {
        return await _context.Payments
            .AnyAsync(p => p.BookingId == bookingId && p.PaymentStatus == "PAID");
    }
}

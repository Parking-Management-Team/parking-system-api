using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;

namespace PBMS.Infrastructure.Repositories;

public class ParkingSessionRepository : BaseRepository<ParkingSessionEntity>, IParkingSessionRepository
{
    public ParkingSessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate)
    {
        var normalized = licensePlate.Trim().ToUpperInvariant();

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
        var normalized = licensePlate.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        var query = _context.Bookings
            .Include(b => b.Vehicle)
            .Include(b => b.VehicleType)
            .Include(b => b.Building)
            .Where(b =>
                b.Vehicle.LicensePlate.ToUpper() == normalized &&
                b.BookingStatus.ToUpper() == "CONFIRMED" &&
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
        var zones = _context.Zones
            .Include(z => z.Floor)
            .Where(z =>
                z.VehicleTypeId == vehicleTypeId &&
                z.AccessType == ZoneAccessType.General &&
                z.Status == ZoneStatus.Available);

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
                    ps.SessionStatus.ToUpper() == "ACTIVE")
            })
            .Where(x => x.ActiveSessions < x.Zone.Capacity)
            .OrderBy(x => x.ActiveSessions)
            .ThenBy(x => x.Zone.Id)
            .Select(x => x.Zone)
            .FirstOrDefaultAsync();
    }

    public async Task<ParkingSlot?> FindAvailableGeneralSlotAsync(int vehicleTypeId, int? buildingId = null)
    {
        var slots = _context.ParkingSlots
            .Include(s => s.Zone)
            .ThenInclude(z => z.Floor)
            .Where(s =>
                s.VehicleTypeId == vehicleTypeId &&
                s.Status == SlotStatus.Available &&
                s.Zone.Status == ZoneStatus.Available &&
                s.Zone.AccessType == ZoneAccessType.General);

        if (buildingId.HasValue)
        {
            slots = slots.Where(s => s.Zone.Floor.BuildingId == buildingId.Value);
        }

        return await slots
            .OrderBy(s => s.ZoneId)
            .ThenBy(s => s.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ParkingSessionEntity?> GetSessionWithDetailsAsync(int id)
    {
        return await _context.ParkingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.Building)
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
}

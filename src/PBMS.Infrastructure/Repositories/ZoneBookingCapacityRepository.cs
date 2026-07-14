using Microsoft.EntityFrameworkCore;
using PBMS.Application.Contracts;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;

namespace PBMS.Infrastructure.Repositories;

public class ZoneBookingCapacityRepository : IZoneBookingCapacityRepository
{
    private readonly AppDbContext _context;

    public ZoneBookingCapacityRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> GetActiveWalkInSessionCountAsync(int zoneId)
    {
        return await _context.ParkingSessions
            .CountAsync(s => s.ZoneId == zoneId 
                && s.BookingId == null 
                && s.SessionStatus == "ACTIVE");
    }

    public async Task<int> GetConfirmedBookingCountAsync(int zoneId, DateTime from, DateTime to, int bufferMinutes)
    {
        var now = DateTime.UtcNow;

        return await _context.Bookings
            .Include(b => b.ParkingSlot)
            .CountAsync(b => b.SlotId != null 
                && b.ParkingSlot!.ZoneId == zoneId
                && (b.BookingStatus == BookingStatus.Confirmed || 
                    (b.BookingStatus == BookingStatus.Pending && b.PaymentDeadline > now))
                && b.PlannedCheckoutTime.AddMinutes(bufferMinutes) > from 
                && to.AddMinutes(bufferMinutes) > b.PlannedCheckinTime);
    }
}

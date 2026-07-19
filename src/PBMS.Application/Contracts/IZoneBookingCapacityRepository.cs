namespace PBMS.Application.Contracts;

/// <summary>
/// Repository interface for computing zone-level booking capacity load.
/// Used by BookingService to enforce BookingLimitRate per Zone.
/// </summary>
public interface IZoneBookingCapacityRepository
{
    /// <summary>
    /// Counts active walk-in parking sessions in the given zone.
    /// A walk-in session is one where <c>BookingId</c> is null and
    /// <c>SessionStatus</c> is Active.
    /// </summary>
    /// <param name="zoneId">The Zone to query.</param>
    Task<int> GetActiveWalkInSessionCountAsync(int zoneId);

    /// <summary>
    /// Counts confirmed (or valid pending) bookings whose time window overlaps
    /// the specified interval <c>[from - bufferMinutes, to + bufferMinutes]</c>
    /// and whose booked slot belongs to the given zone.
    /// </summary>
    /// <param name="zoneId">The Zone to query.</param>
    /// <param name="from">Start of the requested booking window (UTC).</param>
    /// <param name="to">End of the requested booking window (UTC).</param>
    /// <param name="bufferMinutes">Buffer minutes to add around the window edges.</param>
    Task<int> GetConfirmedBookingCountAsync(int zoneId, DateTime from, DateTime to, int bufferMinutes);
}

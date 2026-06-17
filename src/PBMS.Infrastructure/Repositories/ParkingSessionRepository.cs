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
            .AnyAsync(ps => ps.VehicleId == vehicleId && ps.SessionStatus == "Active");
    }

    public async Task<Zone?> FindAvailableZoneAsync(int vehicleTypeId, int? buildingId = null)
    {
        var zones = _context.Zones
            .Include(z => z.Floor)
            .Where(z =>
                z.VehicleTypeId == vehicleTypeId &&
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
                    ps.SessionStatus == "Active")
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
                (s.Zone.Code == "GENERAL" || s.Zone.Name == "GENERAL"));

        if (buildingId.HasValue)
        {
            slots = slots.Where(s => s.Zone.Floor.BuildingId == buildingId.Value);
        }

        return await slots
            .OrderBy(s => s.ZoneId)
            .ThenBy(s => s.Id)
            .FirstOrDefaultAsync();
    }
}

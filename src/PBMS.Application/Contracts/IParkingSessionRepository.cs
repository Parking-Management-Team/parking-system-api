using PBMS.Domain.Entities;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;

namespace PBMS.Application.Contracts;

public interface IParkingSessionRepository : IRepository<ParkingSessionEntity>
{
    Task<VehicleEntity?> GetVehicleByLicensePlateAsync(string licensePlate);

    Task<bool> HasActiveSessionForVehicleAsync(int vehicleId);

    Task<bool> HasActiveSessionForSlotAsync(int slotId);

    Task<bool> HasParkingSessionForBookingAsync(int bookingId);

    Task<PBMS.Domain.Entities.Booking?> GetBookingForCheckInAsync(int bookingId);

    Task<MonthlySubscription?> GetMonthlySubscriptionForCheckInAsync(int monthlySubscriptionId);

    Task<Zone?> FindAvailableZoneAsync(int vehicleTypeId, int? buildingId = null);

    Task<ParkingSlot?> FindAvailableGeneralSlotAsync(int vehicleTypeId, int? buildingId = null);
}

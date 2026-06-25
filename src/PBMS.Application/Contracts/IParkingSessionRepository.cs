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

    Task<PBMS.Domain.Entities.Booking?> GetActiveBookingForCheckInByLicensePlateAsync(string licensePlate, int? buildingId = null);

    Task<MonthlySubscription?> GetMonthlySubscriptionForCheckInAsync(int monthlySubscriptionId);

    Task<Zone?> FindAvailableZoneAsync(int vehicleTypeId, int? buildingId = null);

    Task<ParkingSlot?> FindAvailableGeneralSlotAsync(int vehicleTypeId, int? buildingId = null);

    /// <summary>
    /// Lấy thông tin phiên gửi xe kèm theo thông tin chi tiết (ví dụ: Vehicle).
    /// </summary>
    Task<ParkingSessionEntity?> GetSessionWithDetailsAsync(int id);

    /// <summary>
    /// Kiểm tra xem phiên gửi xe đã có giao dịch thanh toán thành công (PAID) hay chưa.
    /// </summary>
    Task<bool> HasPaidPaymentForSessionAsync(int sessionId);

    /// <summary>
    /// Tìm các lượt gửi xe active có liên kết booking sắp đến hạn planned checkout (trong vòng warningTimeLimit).
    /// </summary>
    Task<IEnumerable<ParkingSessionEntity>> GetOvertimeWarningSessionsAsync(DateTime warningTimeLimit, DateTime now);
}

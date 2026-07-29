using PBMS.Domain.Entities;
using PBMS.Application.ParkingSession.DTOs;
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
    /// Lấy tất cả slot GENERAL trống cho loại phương tiện, dùng cho random assignment.
    /// </summary>
    Task<List<ParkingSlot>> FindAllAvailableGeneralSlotsAsync(int vehicleTypeId, int? buildingId = null);

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

    /// <summary>
    /// Lấy tất cả phiên gửi xe thuộc về các xe đăng ký dưới AccountId chỉ định.
    /// </summary>
    Task<IEnumerable<ParkingSessionEntity>> GetByAccountIdAsync(int accountId);

    /// <summary>
    /// Returns all active parking sessions with full details (Vehicle, Card, etc.).
    /// </summary>
    Task<IEnumerable<ParkingSessionEntity>> GetActiveSessionsWithDetailsAsync();

    /// <summary>
    /// Returns a lightweight projection for the gate active-session list.
    /// Camera images are intentionally excluded so PostgreSQL does not read large
    /// base64 payloads for every active session.
    /// </summary>
    Task<IEnumerable<ActiveParkingSessionSummaryDto>> GetActiveSessionSummariesAsync();

    /// <summary>
    /// Returns the active parking session currently occupying the specified slot,
    /// including the Vehicle navigation property, or null if the slot is free.
    /// Used by the Auto Delay Fallback logic to provide occupying session details.
    /// </summary>
    /// <param name="slotId">The parking slot to query.</param>
    Task<ParkingSessionEntity?> FindActiveSessionForSlotAsync(int slotId);

    /// <summary>
    /// Kiểm tra xem đặt chỗ đã có giao dịch thanh toán thành công (PAID) hay chưa.
    /// </summary>
    Task<bool> HasPaidPaymentForBookingAsync(int bookingId);
}

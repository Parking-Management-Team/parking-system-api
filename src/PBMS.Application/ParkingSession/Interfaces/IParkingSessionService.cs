using PBMS.Application.Common;
using PBMS.Application.ParkingSession.DTOs;

namespace PBMS.Application.ParkingSession.Interfaces;

public interface IParkingSessionService
{
    Task<BaseResponse<ParkingSessionDto>> CheckInAsync(CheckInRequest request);
    Task<BaseResponse<CheckEntryResult>> CheckEntryConditionsAsync(CheckEntryRequest request);
    Task<BaseResponse<ParkingSessionDto>> UpdateCheckinInfoAsync(int sessionId, UpdateCheckinRequest request);
    Task<BaseResponse<CheckInBookingLookupDto>> GetCheckInBookingByLicensePlateAsync(string licensePlate, int? buildingId = null);
    Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request);
    Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetAllAsync();
    Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetActiveAsync();
    Task<BaseResponse<ParkingSessionDto>> GetByIdAsync(int id);
    Task<BaseResponse<ParkingSessionDto>> AssignSlotAsync(int id, AssignParkingSessionSlotRequest request);
    Task<BaseResponse<ParkingSessionDto>> StartCheckoutAsync(int id, StartCheckoutRequest request);
    Task<BaseResponse<ParkingSessionDto>> CompleteAsync(int id);
    Task<BaseResponse<ParkingSessionDto>> RollbackCheckoutAsync(int id);
    Task SendOvertimeWarningsAsync();
    Task<BaseResponse<ParkingSessionDto>> ReplaceSessionCardAsync(int sessionId, string newCardCode);
}

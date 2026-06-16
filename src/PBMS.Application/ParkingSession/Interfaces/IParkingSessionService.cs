using PBMS.Application.Common;
using PBMS.Application.ParkingSession.DTOs;

namespace PBMS.Application.ParkingSession.Interfaces;

public interface IParkingSessionService
{
    Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request);
    Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetAllAsync();
    Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetActiveAsync();
    Task<BaseResponse<ParkingSessionDto>> GetByIdAsync(int id);
    Task<BaseResponse<ParkingSessionDto>> AssignSlotAsync(int id, AssignParkingSessionSlotRequest request);
    Task<BaseResponse<ParkingSessionDto>> StartCheckoutAsync(int id, StartCheckoutRequest request);
    Task<BaseResponse<ParkingSessionDto>> CompleteAsync(int id);
    Task<BaseResponse<ParkingSessionDto>> RollbackCheckoutAsync(int id);
}

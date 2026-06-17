using PBMS.Application.ParkingSession.DTOs;

namespace PBMS.Application.ParkingSession.Interfaces;

public interface IParkingSessionService
{
    Task<ParkingSessionDto> CheckInAsync(CheckInRequest request);
}

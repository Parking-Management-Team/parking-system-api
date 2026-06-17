using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;

namespace PBMS.API.Controllers;

[ApiController]
[Route("api/parking-sessions")]
public class ParkingSessionsController : ControllerBase
{
    private readonly IParkingSessionService _parkingSessionService;

    public ParkingSessionsController(IParkingSessionService parkingSessionService)
    {
        _parkingSessionService = parkingSessionService;
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<BaseResponse<ParkingSessionDto>>> CheckIn([FromBody] CheckInRequest request)
    {
        var session = await _parkingSessionService.CheckInAsync(request);

        return Created(
            $"/api/parking-sessions/{session.Id}",
            BaseResponse<ParkingSessionDto>.Ok(session, "Vehicle checked in successfully."));
    }
}

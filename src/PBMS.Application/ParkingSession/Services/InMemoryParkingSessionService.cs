using PBMS.Application.Common;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;

namespace PBMS.Application.ParkingSession.Services;

public class InMemoryParkingSessionService : IParkingSessionService
{
    private readonly List<ParkingSessionDto> _sessions = new();
    private readonly object _sync = new();
    private int _nextId = 1;

    public Task<BaseResponse<ParkingSessionDto>> CheckInAsync(CheckInRequest request)
    {
        var createRequest = new CreateParkingSessionRequest
        {
            VehicleId = 1,
            BuildingId = request.BuildingId ?? 1,
            CardId = 1,
            BookingId = request.BookingId,
            MonthlySubscriptionId = request.MonthlySubscriptionId,
            InStaffId = request.StaffId,
            CheckInTime = DateTime.UtcNow,
            LicensePlateIn = request.LicensePlate
        };

        return CreateAsync(createRequest);
    }

    public Task<BaseResponse<CheckInBookingLookupDto>> GetCheckInBookingByLicensePlateAsync(string licensePlate, int? buildingId = null)
    {
        return Task.FromResult(BaseResponse<CheckInBookingLookupDto>.Fail(
            "BOOKING_NOT_FOUND",
            "No confirmed booking found for this license plate."));
    }

    public Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request)
    {
        lock (_sync)
        {
            if (request.BookingId.HasValue && request.MonthlySubscriptionId.HasValue)
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail(
                    "INVALID_SESSION_SOURCE",
                    "Booking and monthly subscription cannot both be set."));
            }

            if (HasActive(s => s.VehicleId == request.VehicleId))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail(
                    "VEHICLE_IN_ACTIVE_SESSION",
                    "Vehicle already has an active parking session."));
            }

            if (HasActive(s => s.CardId == request.CardId))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail(
                    "CARD_IN_ACTIVE_SESSION",
                    "Card already has an active parking session."));
            }

            if (request.SlotId.HasValue && HasActive(s => s.SlotId == request.SlotId))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail(
                    "SLOT_IN_ACTIVE_SESSION",
                    "Slot already has an active parking session."));
            }

            var session = new ParkingSessionDto
            {
                Id = _nextId++,
                VehicleId = request.VehicleId,
                BuildingId = request.BuildingId,
                CardId = request.CardId,
                ZoneId = request.ZoneId,
                SlotId = request.SlotId,
                BookingId = request.BookingId,
                BookingCode = request.BookingId.HasValue ? FormatBookingCode(request.BookingId.Value) : null,
                MonthlySubscriptionId = request.MonthlySubscriptionId,
                InStaffId = request.InStaffId,
                CheckInTime = ToUtc(request.CheckInTime ?? DateTime.UtcNow),
                LicensePlateIn = request.LicensePlateIn.Trim().ToUpperInvariant(),
                SessionStatus = "ACTIVE"
            };

            _sessions.Add(session);
            return Task.FromResult(BaseResponse<ParkingSessionDto>.Ok(session, "Created parking session successfully."));
        }
    }

    public Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetAllAsync()
    {
        lock (_sync)
        {
            return Task.FromResult(BaseResponse<IEnumerable<ParkingSessionDto>>.Ok(_sessions.ToList()));
        }
    }

    public Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetActiveAsync()
    {
        lock (_sync)
        {
            return Task.FromResult(BaseResponse<IEnumerable<ParkingSessionDto>>.Ok(
                _sessions.Where(IsActive).ToList()));
        }
    }

    public Task<BaseResponse<ParkingSessionDto>> GetByIdAsync(int id)
    {
        lock (_sync)
        {
            var session = Find(id);
            return Task.FromResult(session == null
                ? BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.")
                : BaseResponse<ParkingSessionDto>.Ok(session));
        }
    }

    public Task<BaseResponse<ParkingSessionDto>> AssignSlotAsync(int id, AssignParkingSessionSlotRequest request)
    {
        lock (_sync)
        {
            var session = Find(id);
            if (session == null)
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found."));
            }

            if (!IsActive(session))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be updated."));
            }

            if (request.SlotId.HasValue && HasActive(s => s.Id != id && s.SlotId == request.SlotId))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("SLOT_IN_ACTIVE_SESSION", "Slot already has an active parking session."));
            }

            session.ZoneId = request.ZoneId;
            session.SlotId = request.SlotId;
            return Task.FromResult(BaseResponse<ParkingSessionDto>.Ok(session, "Assigned parking slot successfully."));
        }
    }

    public Task<BaseResponse<ParkingSessionDto>> StartCheckoutAsync(int id, StartCheckoutRequest request)
    {
        lock (_sync)
        {
            var session = Find(id);
            if (session == null)
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found."));
            }

            if (!IsActive(session))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can start checkout."));
            }

            session.CheckOutTime = ToUtc(request.CheckOutTime ?? DateTime.UtcNow);
            session.LicensePlateOut = string.IsNullOrWhiteSpace(request.LicensePlateOut)
                ? session.LicensePlateIn
                : request.LicensePlateOut.Trim().ToUpperInvariant();
            session.OutStaffId = request.OutStaffId;
            return Task.FromResult(BaseResponse<ParkingSessionDto>.Ok(session, "Started checkout successfully."));
        }
    }

    public Task<BaseResponse<ParkingSessionDto>> CompleteAsync(int id)
    {
        lock (_sync)
        {
            var session = Find(id);
            if (session == null)
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found."));
            }

            if (!IsActive(session))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be completed."));
            }

            session.CheckOutTime ??= DateTime.UtcNow;
            session.LicensePlateOut ??= session.LicensePlateIn;
            session.SessionStatus = "COMPLETED";
            return Task.FromResult(BaseResponse<ParkingSessionDto>.Ok(session, "Completed parking session successfully."));
        }
    }

    public Task<BaseResponse<ParkingSessionDto>> RollbackCheckoutAsync(int id)
    {
        lock (_sync)
        {
            var session = Find(id);
            if (session == null)
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found."));
            }

            if (!IsActive(session))
            {
                return Task.FromResult(BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can rollback checkout."));
            }

            session.CheckOutTime = null;
            session.LicensePlateOut = null;
            session.OutStaffId = null;
            return Task.FromResult(BaseResponse<ParkingSessionDto>.Ok(session, "Rolled back checkout successfully."));
        }
    }

    public Task SendOvertimeWarningsAsync()
    {
        return Task.CompletedTask;
    }

    private ParkingSessionDto? Find(int id) => _sessions.FirstOrDefault(s => s.Id == id);

    private bool HasActive(Func<ParkingSessionDto, bool> predicate) => _sessions.Any(s => IsActive(s) && predicate(s));

    private static bool IsActive(ParkingSessionDto session) =>
        string.Equals(session.SessionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string FormatBookingCode(int bookingId) => $"BK-{bookingId:D6}";
}

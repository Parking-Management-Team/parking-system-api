using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using BookingEntity = PBMS.Domain.Entities.Booking;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;

namespace PBMS.Application.ParkingSession.Services;

public class ParkingSessionService : IParkingSessionService
{
    private const string ActiveStatus = "ACTIVE";
    private const string CompletedStatus = "COMPLETED";

    private readonly IParkingSessionRepository _sessionRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepository;
    private readonly IRepository<BookingEntity> _bookingRepository;
    private readonly IFeeCalculationService _feeCalculationService;
    private readonly ICardRepository _cardRepository;
    private readonly IMonthlySubscriptionRepository _subscriptionRepository;
    private readonly IParkingSlotRepository _parkingSlotRepository;
    private readonly IIncidentRepository _incidentRepository;

    public ParkingSessionService(
        IParkingSessionRepository sessionRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleTypeEntity> vehicleTypeRepository,
        IRepository<BookingEntity> bookingRepository,
        IFeeCalculationService feeCalculationService,
        ICardRepository cardRepository,
        IMonthlySubscriptionRepository subscriptionRepository,
        IParkingSlotRepository parkingSlotRepository,
        IIncidentRepository incidentRepository)
    {
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _bookingRepository = bookingRepository;
        _feeCalculationService = feeCalculationService;
        _cardRepository = cardRepository;
        _subscriptionRepository = subscriptionRepository;
        _parkingSlotRepository = parkingSlotRepository;
        _incidentRepository = incidentRepository;
    }

    public async Task<BaseResponse<ParkingSessionDto>> CheckInAsync(CheckInRequest request)
    {
        if (request.BookingId.HasValue && request.MonthlySubscriptionId.HasValue)
        {
            return BaseResponse<ParkingSessionDto>.Fail("INVALID_SESSION_SOURCE", "Booking and monthly subscription cannot both be set.");
        }

        var normalizedPlate = Normalize(request.LicensePlate);
        var normalizedCardCode = Normalize(request.CardCode);
        var checkInTime = DateTime.UtcNow;

        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Vehicle type with ID {request.VehicleTypeId} not found.");
        }

        var card = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        if (card == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Card with code '{normalizedCardCode}' not found.");
        }

        var isMonthlyCard = string.Equals(card.CardStatus, CardStatus.Assigned.ToString(), StringComparison.OrdinalIgnoreCase);
        if (!isMonthlyCard && !string.Equals(card.CardStatus, CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_NOT_AVAILABLE", "Card is not available for check-in.");
        }

        BookingEntity? booking = null;
        MonthlySubscription? monthlySubscription = null;
        MonthlySubscription? activeSubscription = null;

        if (isMonthlyCard && !request.BookingId.HasValue && !request.MonthlySubscriptionId.HasValue)
        {
            activeSubscription = await _subscriptionRepository.GetActiveSubscriptionByCardIdAsync(card.Id);
            if (activeSubscription == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SUBSCRIPTION_NOT_FOUND", "No active monthly subscription found for this card.");
            }

            if (!string.Equals(Normalize(activeSubscription.Vehicle.LicensePlate), normalizedPlate, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<ParkingSessionDto>.Fail("LICENSE_PLATE_MISMATCH", "License plate does not match the monthly subscription.");
            }

            if (activeSubscription.Vehicle.VehicleTypeId != request.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_TYPE_MISMATCH", "Vehicle type does not match the monthly subscription.");
            }

            if (request.BuildingId.HasValue && activeSubscription.BuildingId != request.BuildingId.Value)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BUILDING_MISMATCH", "Monthly subscription is not valid for this building.");
            }

            if (activeSubscription.ActivatedAt.HasValue && activeSubscription.ActivatedAt.Value > checkInTime ||
                activeSubscription.ExpiredAt.HasValue && activeSubscription.ExpiredAt.Value < checkInTime)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SUBSCRIPTION_EXPIRED", "Monthly subscription has expired or is not yet active.");
            }
        }

        var vehicle = await _sessionRepository.GetVehicleByLicensePlateAsync(normalizedPlate);

        if (request.BookingId.HasValue)
        {
            booking = await _sessionRepository.GetBookingForCheckInAsync(request.BookingId.Value);
            if (booking == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Booking with ID {request.BookingId.Value} not found.");
            }

            if (!StatusEquals(booking.BookingStatus, "CONFIRMED"))
            {
                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_NOT_CONFIRMED", "Only confirmed bookings can be checked in.");
            }

            if (booking.CheckinGraceUntil < checkInTime)
            {
                booking.BookingStatus = "Expired";
                _bookingRepository.Update(booking);
                await _bookingRepository.SaveChangesAsync();

                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_EXPIRED", "Booking has expired and cannot be checked in.");
            }

            if (await _sessionRepository.HasParkingSessionForBookingAsync(booking.Id))
            {
                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_ALREADY_CHECKED_IN", "Booking already has a parking session.");
            }

            var bookingPlate = Normalize(booking.Vehicle.LicensePlate);
            if (booking.VehicleTypeId != request.VehicleTypeId || booking.Vehicle.VehicleTypeId != request.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_VEHICLE_TYPE_MISMATCH", "Booking vehicle type does not match the check-in request.");
            }

            if (!string.Equals(bookingPlate, normalizedPlate, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_LICENSE_PLATE_MISMATCH", "License plate does not match the booking vehicle.");
            }

            if (request.BuildingId.HasValue && booking.BuildingId != request.BuildingId.Value)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BOOKING_BUILDING_MISMATCH", "Booking building does not match the check-in request.");
            }

            vehicle = booking.Vehicle;
        }
        else if (request.MonthlySubscriptionId.HasValue)
        {
            monthlySubscription = await _sessionRepository.GetMonthlySubscriptionForCheckInAsync(request.MonthlySubscriptionId.Value);
            if (monthlySubscription == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Monthly subscription with ID {request.MonthlySubscriptionId.Value} not found.");
            }

            if (!StatusEquals(monthlySubscription.MonthlySubscriptionStatus, ActiveStatus))
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_SUBSCRIPTION_NOT_ACTIVE", "Only active monthly subscriptions can be checked in.");
            }

            if (monthlySubscription.ActivatedAt.HasValue && monthlySubscription.ActivatedAt.Value > checkInTime ||
                monthlySubscription.ExpiredAt.HasValue && monthlySubscription.ExpiredAt.Value < checkInTime)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_SUBSCRIPTION_NOT_VALID", "Monthly subscription is not valid at the check-in time.");
            }

            if (monthlySubscription.Vehicle.VehicleTypeId != request.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_VEHICLE_TYPE_MISMATCH", "Monthly subscription vehicle type does not match the check-in request.");
            }

            if (!string.Equals(Normalize(monthlySubscription.Vehicle.LicensePlate), normalizedPlate, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_LICENSE_PLATE_MISMATCH", "License plate does not match the monthly subscription vehicle.");
            }

            if (request.BuildingId.HasValue && monthlySubscription.BuildingId != request.BuildingId.Value)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_BUILDING_MISMATCH", "Monthly subscription building does not match the check-in request.");
            }

            if (monthlySubscription.AssignedCardId != card.Id)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_CARD_MISMATCH", "Card does not match the monthly subscription assigned card.");
            }

            vehicle = monthlySubscription.Vehicle;
        }

        if (vehicle != null && vehicle.VehicleTypeId != request.VehicleTypeId)
        {
            return BaseResponse<ParkingSessionDto>.Fail("LICENSE_PLATE_TYPE_MISMATCH", "License plate already exists with a different vehicle type.");
        }

        if (vehicle != null && await _sessionRepository.HasActiveSessionForVehicleAsync(vehicle.Id))
        {
            return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_IN_ACTIVE_SESSION", "Vehicle already has an active parking session.");
        }

        vehicle ??= new VehicleEntity
        {
            LicensePlate = normalizedPlate,
            VehicleTypeId = request.VehicleTypeId,
            VehicleStatus = VehicleEntity.StatusActive,
            RegisteredDay = DateTime.UtcNow.Date
        };

        if (vehicle.Id == 0)
        {
            await _vehicleRepository.AddAsync(vehicle);
        }

        Zone? assignedZone = null;
        ParkingSlot? assignedSlot = null;
        var effectiveMonthlySubscription = monthlySubscription ?? activeSubscription;

        if (effectiveMonthlySubscription != null && IsCar(vehicleType))
        {
            if (!effectiveMonthlySubscription.AssignedSlotId.HasValue)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_SLOT_NOT_ASSIGNED", "Car monthly subscription must have an assigned slot before check-in.");
            }

            assignedSlot = await _parkingSlotRepository.GetSlotWithDetailsAsync(effectiveMonthlySubscription.AssignedSlotId.Value);
            if (assignedSlot == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_FOUND", "Assigned monthly slot was not found.");
            }

            if (assignedSlot.VehicleTypeId != request.VehicleTypeId ||
                assignedSlot.Zone.AccessType != ZoneAccessType.Monthly ||
                assignedSlot.Zone.Floor.BuildingId != effectiveMonthlySubscription.BuildingId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_SLOT_INVALID", "Monthly subscription assigned slot is not valid for this vehicle and building.");
            }

            if (assignedSlot.Status is SlotStatus.Blocked or SlotStatus.Maintenance ||
                await _sessionRepository.HasActiveSessionForSlotAsync(assignedSlot.Id))
            {
                return BaseResponse<ParkingSessionDto>.Fail("MONTHLY_SLOT_NOT_AVAILABLE", "Monthly subscription assigned slot is not available for check-in.");
            }

            assignedZone = assignedSlot.Zone;
            assignedSlot.Status = SlotStatus.Occupied;
            _parkingSlotRepository.Update(assignedSlot);
        }
        else if (effectiveMonthlySubscription != null)
        {
            assignedZone = await _sessionRepository.FindAvailableZoneAsync(
                request.VehicleTypeId,
                effectiveMonthlySubscription.BuildingId);

            if (assignedZone == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_ZONE", "No available zone found for this vehicle type.");
            }
        }
        else if (IsCar(vehicleType))
        {
            assignedSlot = await _sessionRepository.FindAvailableGeneralSlotAsync(
                request.VehicleTypeId,
                booking?.BuildingId ?? request.BuildingId);

            if (assignedSlot == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_SLOT", "No available GENERAL slot found for this vehicle type.");
            }

            assignedZone = assignedSlot.Zone;
            assignedSlot.Status = SlotStatus.Occupied;
        }
        else
        {
            assignedZone = await _sessionRepository.FindAvailableZoneAsync(
                request.VehicleTypeId,
                booking?.BuildingId ?? request.BuildingId);

            if (assignedZone == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_ZONE", "No available zone found for this vehicle type.");
            }
        }

        var buildingId = request.BuildingId ?? assignedZone.Floor.BuildingId;

        BookingEntity? activeBooking = null;
        if (!isMonthly)
        {
            var now = DateTime.UtcNow;
            activeBooking = await _bookingRepository.FirstOrDefaultAsync(b =>
                b.Vehicle.LicensePlate.ToUpper() == normalizedPlate &&
                b.BuildingId == buildingId &&
                b.BookingStatus == BookingStatus.Confirmed &&
                b.PlannedCheckinTime.AddMinutes(-30) <= now &&
                b.CheckinGraceUntil >= now);
        }

        var session = new ParkingSessionEntity
        {
            Vehicle = vehicle,
            Card = card,
            Zone = assignedZone,
            ParkingSlot = assignedSlot,
            BuildingId = buildingId,
            CardId = card.Id,
            ZoneId = assignedZone.Id,
            SlotId = assignedSlot?.Id,
            BookingId = booking?.Id,
            MonthlySubscriptionId = effectiveMonthlySubscription?.Id,
            CheckInTime = checkInTime,
            InStaffId = request.StaffId,
            LicensePlateIn = normalizedPlate,
            SessionStatus = ActiveStatus,
            MonthlySubscriptionId = activeSubscription?.Id,
            BookingId = activeBooking?.Id
        };

        if (activeBooking != null)
        {
            activeBooking.BookingStatus = BookingStatus.CheckedIn;
            _bookingRepository.Update(activeBooking);
        }

        if (!isMonthly)
        {
            card.CardStatus = CardStatus.Active.ToString();
            _cardRepository.Update(card);
        }

        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();

        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Vehicle checked in successfully.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request)
    {
        if (request.BookingId.HasValue && request.MonthlySubscriptionId.HasValue)
        {
            return BaseResponse<ParkingSessionDto>.Fail("INVALID_SESSION_SOURCE", "Booking and monthly subscription cannot both be set.");
        }

        if (await _sessionRepository.AnyAsync(s => s.VehicleId == request.VehicleId && s.SessionStatus.ToUpper() == ActiveStatus))
        {
            return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_IN_ACTIVE_SESSION", "Vehicle already has an active parking session.");
        }

        if (await _sessionRepository.AnyAsync(s => s.CardId == request.CardId && s.SessionStatus.ToUpper() == ActiveStatus))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_IN_ACTIVE_SESSION", "Card already has an active parking session.");
        }

        if (request.SlotId.HasValue &&
            await _sessionRepository.AnyAsync(s => s.SlotId == request.SlotId && s.SessionStatus.ToUpper() == ActiveStatus))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SLOT_IN_ACTIVE_SESSION", "Slot already has an active parking session.");
        }

        var session = new ParkingSessionEntity
        {
            VehicleId = request.VehicleId,
            BuildingId = request.BuildingId,
            CardId = request.CardId,
            ZoneId = request.ZoneId,
            SlotId = request.SlotId,
            BookingId = request.BookingId,
            MonthlySubscriptionId = request.MonthlySubscriptionId,
            InStaffId = request.InStaffId,
            CheckInTime = ToUtc(request.CheckInTime ?? DateTime.UtcNow),
            LicensePlateIn = Normalize(request.LicensePlateIn),
            SessionStatus = ActiveStatus
        };

        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Created parking session successfully.");
    }

    public async Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetAllAsync()
    {
        var sessions = await _sessionRepository.GetAllAsync();
        return BaseResponse<IEnumerable<ParkingSessionDto>>.Ok(sessions.Select(Map).ToList());
    }

    public async Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetActiveAsync()
    {
        var sessions = await _sessionRepository.FindAsync(s => s.SessionStatus.ToUpper() == ActiveStatus);
        return BaseResponse<IEnumerable<ParkingSessionDto>>.Ok(sessions.Select(Map).ToList());
    }

    public async Task<BaseResponse<ParkingSessionDto>> GetByIdAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        return session == null
            ? BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.")
            : BaseResponse<ParkingSessionDto>.Ok(Map(session));
    }

    public async Task<BaseResponse<ParkingSessionDto>> AssignSlotAsync(int id, AssignParkingSessionSlotRequest request)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be updated.");
        }

        if (request.SlotId.HasValue &&
            await _sessionRepository.AnyAsync(s => s.Id != id && s.SlotId == request.SlotId && s.SessionStatus.ToUpper() == ActiveStatus))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SLOT_IN_ACTIVE_SESSION", "Slot already has an active parking session.");
        }

        session.ZoneId = request.ZoneId;
        session.SlotId = request.SlotId;
        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Assigned parking slot successfully.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> StartCheckoutAsync(int id, StartCheckoutRequest request)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can start checkout.");
        }

        var checkOutTime = ToUtc(request.CheckOutTime ?? DateTime.UtcNow);
        session.CheckOutTime = checkOutTime;
        session.LicensePlateOut = string.IsNullOrWhiteSpace(request.LicensePlateOut)
            ? session.LicensePlateIn
            : Normalize(request.LicensePlateOut);
        session.OutStaffId = request.OutStaffId;

        if (session.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(session.BookingId.Value);
            var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId);

            if (booking != null && vehicle != null)
            {
                var feeResult = await _feeCalculationService.CalculateFeeAsync(vehicle.VehicleTypeId, session.CheckInTime, checkOutTime);
                decimal realFee = feeResult.TotalFee;
                decimal deposit = booking.DepositAmount;

                decimal amountDue = Math.Max(0, realFee - deposit);

                if (amountDue == 0)
                {
                    // LƯU Ý: Không giải phóng Slot và Card ở đây nữa theo yêu cầu Task 4.
                    // Việc giải phóng sẽ được thực hiện tại CompleteAsync.
                    // Tương tự, không chuyển status sang Completed để Frontend có thể gọi CompleteAsync.
                }
            }
        }

        if (session.MonthlySubscriptionId.HasValue)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(session.MonthlySubscriptionId.Value);
            if (subscription != null && subscription.ExpiredAt.HasValue && checkOutTime <= subscription.ExpiredAt.Value)
            {
                if (checkOutTime <= subscription.ExpiredAt)
                {
                    // Vé tháng còn hiệu lực
                    // Không chuyển status sang Completed và không giải phóng Slot/Card ở đây.
                }
            }
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Started checkout successfully. Waiting for completion.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> CompleteAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be completed.");
        }

        session.CheckOutTime ??= DateTime.UtcNow;
        session.LicensePlateOut ??= session.LicensePlateIn;
        session.SessionStatus = CompletedStatus;

        if (session.SlotId.HasValue)
        {
            var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId.Value);
            if (slot != null)
            {
                slot.Status = SlotStatus.Available;
                _parkingSlotRepository.Update(slot);
            }
        }

        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card != null && card.CardStatus == CardStatus.Active.ToString())
        {
            card.CardStatus = CardStatus.Available.ToString();
            _cardRepository.Update(card);
        }

        // Cập nhật trạng thái Booking nếu có (đã xử lý CheckedIn tại Check-in)

        // Tự động giải quyết sự cố "Mất thẻ" (Lost Card) nếu có
        var sessionIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(id);
        if (sessionIncidents != null)
        {
            var lostCardIncidents = sessionIncidents.Where(i => 
                i.Status == IncidentStatus.Open && 
                i.IncidentType != null && 
                i.IncidentType.IncidentCode.Equals("LOST_CARD", StringComparison.OrdinalIgnoreCase));

            foreach (var incident in lostCardIncidents)
            {
                incident.Status = IncidentStatus.Resolved;
                incident.ResolvedAt = DateTime.UtcNow;
                _incidentRepository.Update(incident);
            }
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Completed parking session successfully.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> RollbackCheckoutAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can rollback checkout.");
        }

        session.CheckOutTime = null;
        session.LicensePlateOut = null;
        session.OutStaffId = null;
        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Rolled back checkout successfully.");
    }

    private static bool IsActive(ParkingSessionEntity session) =>
        string.Equals(session.SessionStatus, ActiveStatus, StringComparison.OrdinalIgnoreCase);

    private static bool StatusEquals(string value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static bool IsCar(VehicleTypeEntity vehicleType) =>
        string.Equals(vehicleType.TypeName, VehicleTypeEntity.CarTypeName, StringComparison.OrdinalIgnoreCase) ||
        vehicleType.TypeName.Contains("CAR", StringComparison.OrdinalIgnoreCase) ||
        vehicleType.TypeName.Contains("AUTO", StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static ParkingSessionDto Map(ParkingSessionEntity session) => new()
    {
        Id = session.Id,
        VehicleId = session.VehicleId,
        BuildingId = session.BuildingId,
        CardId = session.CardId,
        ZoneId = session.ZoneId,
        SlotId = session.SlotId,
        BookingId = session.BookingId,
        MonthlySubscriptionId = session.MonthlySubscriptionId,
        InStaffId = session.InStaffId,
        OutStaffId = session.OutStaffId,
        CheckInTime = session.CheckInTime,
        CheckOutTime = session.CheckOutTime,
        LicensePlateIn = session.LicensePlateIn,
        LicensePlateOut = session.LicensePlateOut,
        SessionStatus = session.SessionStatus,
        CardCode = session.Card?.CardCode,
        ZoneCode = session.Zone?.Code,
        SlotCode = session.ParkingSlot?.Code
    };
}

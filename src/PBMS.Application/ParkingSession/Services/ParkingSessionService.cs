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
        var normalizedPlate = Normalize(request.LicensePlate);
        var normalizedCardCode = Normalize(request.CardCode);

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

        bool isMonthly = string.Equals(card.CardStatus, CardStatus.Assigned.ToString(), StringComparison.OrdinalIgnoreCase);

        // 1. Kiểm tra trạng thái thẻ đỗ xe
        if (!isMonthly && !string.Equals(card.CardStatus, CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_NOT_AVAILABLE", "Card is not available for check-in.");
        }

        MonthlySubscription? activeSubscription = null;
        if (isMonthly)
        {
            activeSubscription = await _subscriptionRepository.GetActiveSubscriptionByCardIdAsync(card.Id);
            if (activeSubscription == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SUBSCRIPTION_NOT_FOUND", "No active monthly subscription found for this card.");
            }

            // Đối chiếu biển số, loại xe, tòa nhà, thời hạn hiệu lực
            var now = DateTime.UtcNow;
            if (Normalize(activeSubscription.Vehicle.LicensePlate) != normalizedPlate)
            {
                return BaseResponse<ParkingSessionDto>.Fail("LICENSE_PLATE_MISMATCH", "License plate does not match the monthly subscription.");
            }

            if (activeSubscription.Vehicle.VehicleTypeId != request.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_TYPE_MISMATCH", "Vehicle type does not match the monthly subscription.");
            }

            if (activeSubscription.BuildingId != request.BuildingId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BUILDING_MISMATCH", "Monthly subscription is not valid for this building.");
            }

            if (activeSubscription.ActivatedAt > now || activeSubscription.ExpiredAt < now)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SUBSCRIPTION_EXPIRED", "Monthly subscription has expired or is not yet active.");
            }
        }

        // 2. Kiểm tra phương tiện đã ở trong bãi chưa
        var vehicle = await _sessionRepository.GetVehicleByLicensePlateAsync(normalizedPlate);
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

        if (isMonthly)
        {
            if (IsCar(vehicleType))
            {
                if (activeSubscription == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("SUBSCRIPTION_NOT_FOUND", "Monthly subscription not found for this card.");
                }

                if (!activeSubscription.AssignedSlotId.HasValue)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_ASSIGNED", "Monthly subscription for car does not have an assigned slot.");
                }

                assignedSlot = await _parkingSlotRepository.GetSlotWithDetailsAsync(activeSubscription.AssignedSlotId.Value);
                if (assignedSlot == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_FOUND", "Assigned monthly slot was not found.");
                }

                assignedZone = assignedSlot.Zone;
                assignedSlot.Status = SlotStatus.Occupied;
                _parkingSlotRepository.Update(assignedSlot);
            }
            else
            {
                assignedZone = await _sessionRepository.FindAvailableZoneAsync(request.VehicleTypeId, request.BuildingId);
                if (assignedZone == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_ZONE", "No available zone found for this vehicle type.");
                }
            }
        }
        else
        {
            if (IsCar(vehicleType))
            {
                assignedSlot = await _sessionRepository.FindAvailableGeneralSlotAsync(request.VehicleTypeId, request.BuildingId);
                if (assignedSlot == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_SLOT", "No available GENERAL slot found for this vehicle type.");
                }

                assignedZone = assignedSlot.Zone;
                assignedSlot.Status = SlotStatus.Occupied;
            }
            else
            {
                assignedZone = await _sessionRepository.FindAvailableZoneAsync(request.VehicleTypeId, request.BuildingId);
                if (assignedZone == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_ZONE", "No available zone found for this vehicle type.");
                }
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
            CheckInTime = DateTime.UtcNow,
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

        // --- BẮT ĐẦU XỬ LÝ KHẤU TRỪ TIỀN CỌC ---
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
        // --- KẾT THÚC XỬ LÝ KHẤU TRỪ ---

        // --- BẮT ĐẦU XỬ LÝ THẺ THÁNG ---
        if (session.MonthlySubscriptionId.HasValue)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(session.MonthlySubscriptionId.Value);
            if (subscription != null)
            {
                if (checkOutTime <= subscription.ExpiredAt)
                {
                    // Vé tháng còn hiệu lực
                    // Không chuyển status sang Completed và không giải phóng Slot/Card ở đây.
                }
            }
        }
        // --- KẾT THÚC XỬ LÝ THẺ THÁNG ---

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

        // Giải phóng slot đỗ xe
        if (session.SlotId.HasValue)
        {
            var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId.Value);
            if (slot != null)
            {
                slot.Status = SlotStatus.Available;
                _parkingSlotRepository.Update(slot);
            }
        }

        // Giải phóng thẻ đỗ xe (chỉ áp dụng cho thẻ thường NORMAL)
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

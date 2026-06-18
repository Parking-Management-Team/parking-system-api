using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
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
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IFeeCalculationService _feeCalculationService;
    private readonly ICardRepository _cardRepository;

    public ParkingSessionService(
        IParkingSessionRepository sessionRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleTypeEntity> vehicleTypeRepository,
        IRepository<Booking> bookingRepository,
        IFeeCalculationService feeCalculationService,
        ICardRepository cardRepository)
    {
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _bookingRepository = bookingRepository;
        _feeCalculationService = feeCalculationService;
        _cardRepository = cardRepository;
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

        if (!string.Equals(card.CardStatus, CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_NOT_AVAILABLE", "Card is not available for check-in.");
        }

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

        Zone? assignedZone;
        ParkingSlot? assignedSlot = null;

        if (IsCar(vehicleType))
        {
            assignedSlot = await _sessionRepository.FindAvailableGeneralSlotAsync(
                request.VehicleTypeId,
                request.BuildingId);

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
                request.BuildingId);

            if (assignedZone == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_ZONE", "No available zone found for this vehicle type.");
            }
        }

        var buildingId = request.BuildingId ?? assignedZone.Floor.BuildingId;
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
            SessionStatus = ActiveStatus
        };

        card.CardStatus = CardStatus.Active.ToString();

        await _sessionRepository.AddAsync(session);
        _cardRepository.Update(card);
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
                // Tính toán phí đỗ xe thực tế của lượt gửi xe
                var feeResult = await _feeCalculationService.CalculateFeeAsync(vehicle.VehicleTypeId, session.CheckInTime, checkOutTime);
                decimal realFee = feeResult.TotalFee;
                decimal deposit = booking.DepositAmount;

                // Tính số tiền còn nợ sau khi lấy Phí thực tế trừ đi Tiền cọc
                decimal amountDue = Math.Max(0, realFee - deposit);

                // Nếu tiền đỗ xe nhỏ hơn hoặc bằng tiền cọc -> số tiền nợ = 0 VNĐ
                if (amountDue == 0)
                {
                    // Đóng lượt đỗ và booking ngay lập tức, không cần tạo payment
                    session.SessionStatus = "COMPLETED";
                    booking.BookingStatus = "Completed";

                    _sessionRepository.Update(session);
                    _bookingRepository.Update(booking);
                    await _sessionRepository.SaveChangesAsync();

                    return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Succesfull. Parking fee is fully deducted by deposit. Parking session completed immediately.");
                }
            }
        }
        // --- KẾT THÚC XỬ LÝ KHẤU TRỪ ---

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Started checkout successfully.");
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

using PBMS.Application.Common;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Application.ParkingSession.Services;

public class ParkingSessionService : IParkingSessionService
{
    private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IMonthlySubscriptionRepository _monthlySubRepository;

    public ParkingSessionService(
        IRepository<PBMS.Domain.Entities.ParkingSession> sessionRepository,
        ICardRepository cardRepository,
        IMonthlySubscriptionRepository monthlySubRepository)
    {
        _sessionRepository = sessionRepository;
        _cardRepository = cardRepository;
        _monthlySubRepository = monthlySubRepository;
    }

    public async Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request)
    {
        if (request.BookingId.HasValue && request.MonthlySubscriptionId.HasValue)
        {
            return BaseResponse<ParkingSessionDto>.Fail("INVALID_SESSION_SOURCE", "Booking and monthly subscription cannot both be set.");
        }

        // 1. Kiểm tra thẻ tồn tại trong hệ thống
        var card = await _cardRepository.GetByIdAsync(request.CardId);
        if (card == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_NOT_FOUND", "Thẻ gửi xe không tồn tại.");
        }

        // 2. Kiểm tra thẻ báo mất (Lost)
        if (string.Equals(card.CardStatus, PBMS.Domain.Enums.CardStatus.Lost.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_LOST", "Thẻ đã bị báo mất trên hệ thống.");
        }

        // 3. Kiểm tra thẻ bị khóa (Blocked)
        if (string.Equals(card.CardStatus, PBMS.Domain.Enums.CardStatus.Blocked.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_BLOCKED", "Thẻ đã bị khóa.");
        }

        // 4. Kiểm tra xem thẻ có đang trong một lượt gửi xe ACTIVE khác không (Anti-passback)
        if (string.Equals(card.CardStatus, PBMS.Domain.Enums.CardStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) ||
            await _sessionRepository.AnyAsync(s => s.CardId == request.CardId && s.SessionStatus.ToUpper() == "ACTIVE"))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_IN_ACTIVE_SESSION", "Card already has an active parking session.");
        }

        if (await _sessionRepository.AnyAsync(s => s.VehicleId == request.VehicleId && s.SessionStatus.ToUpper() == "ACTIVE"))
        {
            return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_IN_ACTIVE_SESSION", "Vehicle already has an active parking session.");
        }

        if (request.SlotId.HasValue &&
            await _sessionRepository.AnyAsync(s => s.SlotId == request.SlotId && s.SessionStatus.ToUpper() == "ACTIVE"))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SLOT_IN_ACTIVE_SESSION", "Slot already has an active parking session.");
        }

        // 5. Tự động phát hiện và kiểm tra điều kiện Vé Tháng (nếu thẻ được gán vé tháng)
        var subscription = await _monthlySubRepository.GetActiveSubscriptionByCardCodeAsync(card.CardCode);
        int? finalMonthlySubscriptionId = null;
        int? finalSlotId = request.SlotId;

        if (subscription != null)
        {
            // Kiểm tra xem tòa nhà gửi xe có khớp không
            if (subscription.BuildingId != request.BuildingId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BUILDING_MISMATCH", "Vé tháng không đăng ký tại tòa nhà này.");
            }

            // Kiểm tra xem phương tiện check-in có đúng xe đã đăng ký vé tháng không
            if (subscription.VehicleId != request.VehicleId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_MISMATCH", "Thẻ tháng không khớp với phương tiện đã đăng ký.");
            }

            // Kiểm tra biển số xe trùng khớp
            var cleanLicensePlateIn = (request.LicensePlateIn ?? "").Replace(" ", "").ToUpperInvariant();
            var cleanRegPlate = (subscription.Vehicle?.LicensePlate ?? "").Replace(" ", "").ToUpperInvariant();
            if (cleanLicensePlateIn != cleanRegPlate)
            {
                return BaseResponse<ParkingSessionDto>.Fail("LICENSE_PLATE_MISMATCH", "Biển số xe check-in không khớp với biển số đăng ký vé tháng.");
            }

            finalMonthlySubscriptionId = subscription.Id;
            
            // Tự động phân bổ vị trí đỗ xe cố định (Đối với ô tô đăng ký tháng)
            if (subscription.AssignedSlotId.HasValue)
            {
                finalSlotId = subscription.AssignedSlotId;
            }
        }
        else if (string.Equals(card.CardType, "MONTHLY", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(card.CardType, "MONTHLY_CARD", StringComparison.OrdinalIgnoreCase))
        {
            // Nếu thẻ được gán loại Monthly nhưng không tìm thấy vé tháng hoạt động hợp lệ
            return BaseResponse<ParkingSessionDto>.Fail("NO_ACTIVE_SUBSCRIPTION", "Thẻ tháng không có đăng ký hoạt động hợp lệ.");
        }

        // 6. Tạo phiên gửi xe mới
        var session = new PBMS.Domain.Entities.ParkingSession
        {
            VehicleId = request.VehicleId,
            BuildingId = request.BuildingId,
            CardId = request.CardId,
            ZoneId = request.ZoneId,
            SlotId = finalSlotId,
            BookingId = request.BookingId,
            MonthlySubscriptionId = finalMonthlySubscriptionId,
            InStaffId = request.InStaffId,
            CheckInTime = ToUtc(request.CheckInTime ?? DateTime.UtcNow),
            LicensePlateIn = (request.LicensePlateIn ?? string.Empty).Trim().ToUpperInvariant(),
            SessionStatus = "ACTIVE"
        };

        // 7. Cập nhật trạng thái thẻ sang ACTIVE
        card.CardStatus = PBMS.Domain.Enums.CardStatus.Active.ToString();
        _cardRepository.Update(card);

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
        var sessions = await _sessionRepository.FindAsync(s => s.SessionStatus.ToUpper() == "ACTIVE");
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
            await _sessionRepository.AnyAsync(s => s.Id != id && s.SlotId == request.SlotId && s.SessionStatus.ToUpper() == "ACTIVE"))
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

        session.CheckOutTime = ToUtc(request.CheckOutTime ?? DateTime.UtcNow);
        session.LicensePlateOut = string.IsNullOrWhiteSpace(request.LicensePlateOut)
            ? session.LicensePlateIn
            : request.LicensePlateOut.Trim().ToUpperInvariant();
        session.OutStaffId = request.OutStaffId;
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
        session.SessionStatus = "COMPLETED";

        // Cập nhật trạng thái thẻ về Available
        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card != null)
        {
            card.CardStatus = PBMS.Domain.Enums.CardStatus.Available.ToString();
            _cardRepository.Update(card);
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

        // Cập nhật trạng thái thẻ về lại Active
        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card != null)
        {
            card.CardStatus = PBMS.Domain.Enums.CardStatus.Active.ToString();
            _cardRepository.Update(card);
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Rolled back checkout successfully.");
    }

    private static bool IsActive(PBMS.Domain.Entities.ParkingSession session) =>
        string.Equals(session.SessionStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase);

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static ParkingSessionDto Map(PBMS.Domain.Entities.ParkingSession session) => new()
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
        SessionStatus = session.SessionStatus
    };
}

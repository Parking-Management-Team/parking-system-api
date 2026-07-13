using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Domain.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using BookingEntity = PBMS.Domain.Entities.Booking;
using IncidentEntity = PBMS.Domain.Entities.Incident;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;

namespace PBMS.Application.ParkingSession.Services;

public class ParkingSessionService : IParkingSessionService
{
    private const string ActiveStatus = "ACTIVE";
    private const string CompletedStatus = "COMPLETED";
    private const int LateCheckoutGracePeriodMinutes = 15;

    private readonly IParkingSessionRepository _sessionRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepository;
    private readonly IRepository<BookingEntity> _bookingRepository;
    private readonly IPricingCalculationService _pricingCalculationService;
    private readonly ICardRepository _cardRepository;
    private readonly IParkingSlotRepository _parkingSlotRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<IncidentType> _incidentTypeRepository;
    private readonly IRepository<PenaltyConfig> _penaltyConfigRepository;
    private readonly IBlacklistRepository _blacklistRepository;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IPricingPolicyRepository _pricingPolicyRepository;

    public ParkingSessionService(
        IParkingSessionRepository sessionRepository,
        IRepository<VehicleEntity> vehicleRepository,
        IRepository<VehicleTypeEntity> vehicleTypeRepository,
        IRepository<BookingEntity> bookingRepository,
        IPricingCalculationService pricingCalculationService,
        ICardRepository cardRepository,
        IParkingSlotRepository parkingSlotRepository,
        IIncidentRepository incidentRepository,
        IRepository<IncidentType> incidentTypeRepository,
        IRepository<PenaltyConfig> penaltyConfigRepository,
        IBlacklistRepository blacklistRepository,
        IRepository<Notification> notificationRepository,
        IAccountRepository accountRepository,
        IPricingPolicyRepository pricingPolicyRepository)
    {
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleTypeRepository = vehicleTypeRepository;
        _bookingRepository = bookingRepository;
        _pricingCalculationService = pricingCalculationService;
        _cardRepository = cardRepository;
        _parkingSlotRepository = parkingSlotRepository;
        _incidentRepository = incidentRepository;
        _incidentTypeRepository = incidentTypeRepository;
        _penaltyConfigRepository = penaltyConfigRepository;
        _blacklistRepository = blacklistRepository;
        _notificationRepository = notificationRepository;
        _accountRepository = accountRepository;
        _pricingPolicyRepository = pricingPolicyRepository;
    }

    public async Task<BaseResponse<ParkingSessionDto>> CheckInAsync(CheckInRequest request)
    {


        var normalizedPlate = Normalize(request.LicensePlate);
        var normalizedCardCode = Normalize(request.CardCode);
        var checkInTime = DateTime.UtcNow;

        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Vehicle type with ID {request.VehicleTypeId} not found.");
        }

        // Validate active pricing policy exists
        var activePolicies = await _pricingPolicyRepository.GetAllWithWindowsAsync(request.VehicleTypeId, "Active");
        var applicablePolicies = activePolicies.Where(pp =>
            pp.EffectiveStart <= checkInTime.Date &&
            (pp.EffectiveEnd == null || pp.EffectiveEnd.Value >= checkInTime.Date)
        ).ToList();

        if (applicablePolicies.Count == 0)
        {
            return BaseResponse<ParkingSessionDto>.Fail("PRICING_POLICY_NOT_FOUND", $"No active pricing policy found for vehicle type ID {request.VehicleTypeId} at check-in time.");
        }
        if (applicablePolicies.Count > 1)
        {
            return BaseResponse<ParkingSessionDto>.Fail("MULTIPLE_PRICING_POLICIES", $"Multiple active pricing policies found for vehicle type ID {request.VehicleTypeId} at check-in time.");
        }

        var card = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        if (card == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Card with code '{normalizedCardCode}' not found.");
        }

        var isCardBlacklisted = await _blacklistRepository.AnyAsync(b => b.CardId == card.Id && !b.IsDeleted);
        if (isCardBlacklisted)
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_BLACKLISTED", "Card is blacklisted and cannot be used for check-in.");
        }

        var isCardInActiveSession = await _sessionRepository.AnyAsync(s => s.CardId == card.Id && s.SessionStatus == ActiveStatus);
        if (isCardInActiveSession)
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_IN_ACTIVE_SESSION", "Card is already in use in an active parking session.");
        }

        if (!string.Equals(card.CardStatus, CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BaseResponse<ParkingSessionDto>.Fail("CARD_NOT_AVAILABLE", "Card is not available for check-in.");
        }

        BookingEntity? booking = null;

        var vehicle = await _sessionRepository.GetVehicleByLicensePlateAsync(normalizedPlate);

        var effectiveBookingId = request.BookingId;
        if (!effectiveBookingId.HasValue)
        {
            var plateBooking = await _sessionRepository.GetActiveBookingForCheckInByLicensePlateAsync(normalizedPlate, request.BuildingId);
            effectiveBookingId = plateBooking?.Id;
        }

        if (effectiveBookingId.HasValue)
        {
            booking = await _sessionRepository.GetBookingForCheckInAsync(effectiveBookingId.Value);
            if (booking == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Booking with ID {effectiveBookingId.Value} not found.");
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


        if (vehicle != null && vehicle.VehicleTypeId != request.VehicleTypeId)
        {
            return BaseResponse<ParkingSessionDto>.Fail("LICENSE_PLATE_TYPE_MISMATCH", "License plate already exists with a different vehicle type.");
        }

        if (vehicle != null && await _sessionRepository.HasActiveSessionForVehicleAsync(vehicle.Id))
        {
            return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_IN_ACTIVE_SESSION", "Vehicle already has an active parking session.");
        }

        if (vehicle != null && vehicle.Id > 0)
        {
            var isVehicleBlacklisted = await _blacklistRepository.AnyAsync(b => b.VehicleId == vehicle.Id && !b.IsDeleted);
            if (isVehicleBlacklisted)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_BLACKLISTED", "Vehicle is blacklisted and cannot check in.");
            }
        }

        vehicle ??= new VehicleEntity
        {
            LicensePlate = normalizedPlate,
            VehicleTypeId = request.VehicleTypeId,
            VehicleStatus = VehicleEntity.StatusActive,
            RegisteredDay = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).Date
        };

        if (vehicle.Id == 0)
        {
            await _vehicleRepository.AddAsync(vehicle);
        }

        Zone? assignedZone = null;
        ParkingSlot? assignedSlot = null;

        if (IsCar(vehicleType))
        {
            if (booking != null && (request.OverrideSlotId.HasValue || booking.SlotId.HasValue))
            {
                var targetSlotId = request.OverrideSlotId ?? booking.SlotId!.Value;
                assignedSlot = await _parkingSlotRepository.GetSlotWithDetailsAsync(targetSlotId);
                if (assignedSlot == null)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_FOUND", "Vị trí đỗ được chọn không tồn tại.");
                }

                // Nếu là vị trí đỗ được staff ghi đè (OverrideSlotId) thì phải kiểm tra xem vị trí đỗ đó có trống và không trùng lịch không
                if (request.OverrideSlotId.HasValue)
                {
                    // 1. Kiểm tra xem slot này hiện tại có đang bận không
                    if (assignedSlot.Status is SlotStatus.Blocked or SlotStatus.Maintenance or SlotStatus.Reserved ||
                        await _sessionRepository.HasActiveSessionForSlotAsync(assignedSlot.Id))
                    {
                        return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_AVAILABLE", "Vị trí đỗ được chọn để đổi hiện đang bị chiếm dụng hoặc không khả dụng.");
                    }

                    // 2. Kiểm tra xem slot mới có bị trùng với booking nào khác của slot đó trong khoảng thời gian đỗ của booking này không (bao gồm 30m đệm)
                    var isSlotOverlap = await _bookingRepository.AnyAsync(b =>
                        b.Id != booking.Id &&
                        b.SlotId == assignedSlot.Id &&
                        b.BuildingId == booking.BuildingId &&
                        (b.BookingStatus == BookingStatus.Confirmed || 
                         (b.BookingStatus == BookingStatus.Pending && b.PaymentDeadline > checkInTime)) &&
                        b.PlannedCheckoutTime.AddMinutes(30) > booking.PlannedCheckinTime &&
                        booking.PlannedCheckoutTime.AddMinutes(30) > b.PlannedCheckinTime);

                    if (isSlotOverlap)
                    {
                        return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_AVAILABLE", "Vị trí đỗ được chọn để đổi đã được đặt trước bởi tài xế khác trong khung giờ này.");
                    }

                    // Hợp lệ -> Gán lại Slot cho Booking
                    booking.SlotId = assignedSlot.Id;
                    _bookingRepository.Update(booking);
                }
                else
                {
                    // Nếu là slot đặt trước thông thường mà bị bận -> Trả về lỗi bận để Frontend xử lý đổi slot
                    if (assignedSlot.Status is SlotStatus.Blocked or SlotStatus.Maintenance or SlotStatus.Reserved ||
                        await _sessionRepository.HasActiveSessionForSlotAsync(assignedSlot.Id))
                    {
                        return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_AVAILABLE", "Vị trí đỗ đặt trước hiện đang bị chiếm dụng hoặc không khả dụng.");
                    }
                }

                assignedZone = assignedSlot.Zone;
                assignedSlot.Status = SlotStatus.Occupied;
                _parkingSlotRepository.Update(assignedSlot);
            }
            else if (request.RandomizeSlot)
            {
                // Random slot assignment: fetch all available slots and pick one randomly
                var availableSlots = await _sessionRepository.FindAllAvailableGeneralSlotsAsync(
                    request.VehicleTypeId,
                    booking?.BuildingId ?? request.BuildingId);

                if (availableSlots.Count == 0)
                {
                    return BaseResponse<ParkingSessionDto>.Fail("NO_AVAILABLE_SLOT", "No available GENERAL slot found for this vehicle type.");
                }

                var random = new Random();
                assignedSlot = availableSlots[random.Next(availableSlots.Count)];
                assignedZone = assignedSlot.Zone;
                assignedSlot.Status = SlotStatus.Occupied;
                _parkingSlotRepository.Update(assignedSlot);
            }
            else
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
                _parkingSlotRepository.Update(assignedSlot);
            }
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

        var buildingId = booking?.BuildingId ?? request.BuildingId ?? assignedZone.Floor.BuildingId;

        BookingEntity? activeBooking = booking;
        if (activeBooking == null)
        {
            var now = DateTime.UtcNow;
            activeBooking = await _bookingRepository.FirstOrDefaultAsync(b =>
                b.Vehicle.LicensePlate.ToUpper() == normalizedPlate &&
                b.BuildingId == buildingId &&
                b.BookingStatus == BookingStatus.Confirmed &&
                b.PlannedCheckinTime.AddMinutes(-30) <= now &&
                b.CheckinGraceUntil >= now);

            if (activeBooking != null)
            {
                buildingId = activeBooking.BuildingId;
            }
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
            BookingId = activeBooking?.Id,
            MonthlySubscriptionId = null,
            Booking = booking,
            MonthlySubscription = null,
            CheckInTime = checkInTime,
            InStaffId = request.StaffId,
            LicensePlateIn = normalizedPlate,
            ImageIn = request.ImageIn,
            SessionStatus = ActiveStatus
        };

        if (activeBooking != null)
        {
            activeBooking.BookingStatus = BookingStatus.CheckedIn;
            _bookingRepository.Update(activeBooking);
        }

        card.CardStatus = CardStatus.Active.ToString();
        _cardRepository.Update(card);

        if (booking != null)
        {
            booking.BookingStatus = BookingStatus.CheckedIn;
            _bookingRepository.Update(booking);
        }

        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();

        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Vehicle checked in successfully.");
    }

    public async Task<BaseResponse<CheckEntryResult>> CheckEntryConditionsAsync(CheckEntryRequest request)
    {
        var normalizedPlate = Normalize(request.LicensePlate);
        var normalizedCardCode = Normalize(request.CardCode);

        var result = new CheckEntryResult();

        // 1. Validate card exists and is available
        var card = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        result.CardAvailable = card != null &&
            (string.Equals(card.CardStatus, CardStatus.Available.ToString(), StringComparison.OrdinalIgnoreCase) ||
             string.Equals(card.CardStatus, CardStatus.Assigned.ToString(), StringComparison.OrdinalIgnoreCase));

        if (card == null)
        {
            result.Allowed = false;
            result.Reason = $"Card with code '{normalizedCardCode}' not found.";
            return BaseResponse<CheckEntryResult>.Ok(result, result.Reason);
        }

        // 2. Check blacklist
        var isCardBlacklisted = await _blacklistRepository.AnyAsync(b => b.CardId == card.Id && !b.IsDeleted);
        var isVehicleBlacklisted = false;

        var vehicle = await _sessionRepository.GetVehicleByLicensePlateAsync(normalizedPlate);
        if (vehicle != null && vehicle.Id > 0)
        {
            isVehicleBlacklisted = await _blacklistRepository.AnyAsync(b => b.VehicleId == vehicle.Id && !b.IsDeleted);
        }

        result.NotBlacklisted = !isCardBlacklisted && !isVehicleBlacklisted;

        // 3. Check vehicle type
        var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId);
        if (vehicleType == null)
        {
            result.Allowed = false;
            result.Reason = $"Vehicle type with ID {request.VehicleTypeId} not found.";
            return BaseResponse<CheckEntryResult>.Ok(result, result.Reason);
        }

        // 4. Check vehicle not already in active session
        if (vehicle != null && vehicle.Id > 0)
        {
            result.NotAlreadyParked = !await _sessionRepository.HasActiveSessionForVehicleAsync(vehicle.Id);
        }
        else
        {
            result.NotAlreadyParked = true;
        }

        // 5. Check card not already in active session
        if (card != null)
        {
            var cardInSession = await _sessionRepository.AnyAsync(s => s.CardId == card.Id && s.SessionStatus == ActiveStatus);
            if (cardInSession)
            {
                result.CardAvailable = false;
            }
        }

        // 6. Check zone availability
        if (IsCar(vehicleType))
        {
            var availableSlot = await _sessionRepository.FindAvailableGeneralSlotAsync(
                request.VehicleTypeId, request.BuildingId);
            result.ZoneAvailable = availableSlot != null;
        }
        else
        {
            var availableZone = await _sessionRepository.FindAvailableZoneAsync(
                request.VehicleTypeId, request.BuildingId);
            result.ZoneAvailable = availableZone != null;
        }

        // 7. Check pricing policy validity
        var checkInTime = DateTime.UtcNow.AddHours(7);
        var activePolicies = await _pricingPolicyRepository.GetAllWithWindowsAsync(request.VehicleTypeId, "Active");
        var applicablePolicies = activePolicies.Where(pp =>
            pp.EffectiveStart <= checkInTime.Date &&
            (pp.EffectiveEnd == null || pp.EffectiveEnd.Value >= checkInTime.Date)
        ).ToList();
        result.PricingPolicyValid = applicablePolicies.Count == 1;

        // Determine overall result
        result.Allowed = result.CardAvailable && result.NotBlacklisted && result.NotAlreadyParked && result.ZoneAvailable && result.PricingPolicyValid;

        if (!result.Allowed && string.IsNullOrEmpty(result.Reason))
        {
            var failures = new List<string>();
            if (!result.CardAvailable) failures.Add("card is not available or already in use");
            if (!result.NotBlacklisted) failures.Add("card or vehicle is blacklisted");
            if (!result.NotAlreadyParked) failures.Add("vehicle already has an active session");
            if (!result.ZoneAvailable) failures.Add("no available zone/slot for this vehicle type");
            if (!result.PricingPolicyValid) failures.Add("no active pricing policy or multiple active pricing policies found");
            result.Reason = $"Entry conditions not met: {string.Join("; ", failures)}.";
        }
        else if (result.Allowed)
        {
            result.Reason = "All entry conditions passed. Ready for check-in.";
        }

        return BaseResponse<CheckEntryResult>.Ok(result, result.Reason);
    }

    public async Task<BaseResponse<ParkingSessionDto>> UpdateCheckinInfoAsync(int sessionId, UpdateCheckinRequest request)
    {
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {sessionId} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be updated.");
        }

        // Update license plate
        if (!string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            var normalizedPlate = Normalize(request.LicensePlate);
            session.LicensePlateIn = normalizedPlate;

            // Also update the vehicle's license plate
            if (session.Vehicle != null)
            {
                session.Vehicle.LicensePlate = normalizedPlate;
                _vehicleRepository.Update(session.Vehicle);
            }
        }

        // Update vehicle type
        if (request.VehicleTypeId.HasValue)
        {
            var vehicleType = await _vehicleTypeRepository.GetByIdAsync(request.VehicleTypeId.Value);
            if (vehicleType == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Vehicle type with ID {request.VehicleTypeId.Value} not found.");
            }

            if (session.Vehicle != null)
            {
                session.Vehicle.VehicleTypeId = request.VehicleTypeId.Value;
                _vehicleRepository.Update(session.Vehicle);
            }
        }

        // Update card
        if (!string.IsNullOrWhiteSpace(request.CardCode))
        {
            var normalizedCardCode = Normalize(request.CardCode);
            var newCard = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
            if (newCard == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Card with code '{normalizedCardCode}' not found.");
            }

            if (newCard.Id != session.CardId)
            {
                // Release old card
                var oldCard = await _cardRepository.GetByIdAsync(session.CardId);
                if (oldCard != null && string.Equals(oldCard.CardStatus, CardStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    oldCard.CardStatus = CardStatus.Available.ToString();
                    _cardRepository.Update(oldCard);
                }

                // Assign new card
                newCard.CardStatus = CardStatus.Active.ToString();
                _cardRepository.Update(newCard);
                session.CardId = newCard.Id;
            }
        }

        // Update slot
        if (request.SlotId.HasValue)
        {
            var newSlot = await _parkingSlotRepository.GetSlotWithDetailsAsync(request.SlotId.Value);
            if (newSlot == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_FOUND", $"Parking slot with ID {request.SlotId.Value} not found.");
            }

            if (newSlot.Status is SlotStatus.Blocked or SlotStatus.Maintenance or SlotStatus.Reserved)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_AVAILABLE", "Selected slot is currently blocked, under maintenance, or reserved.");
            }

            if (session.Vehicle != null && newSlot.VehicleTypeId != session.Vehicle.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_TYPE_MISMATCH", "Selected slot does not match the vehicle type.");
            }

            if (newSlot.Zone?.Floor != null && newSlot.Zone.Floor.BuildingId != session.BuildingId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BUILDING_MISMATCH", "Selected slot is in a different building.");
            }

            // Release old slot
            if (session.SlotId.HasValue && session.SlotId.Value != request.SlotId.Value)
            {
                var oldSlot = await _parkingSlotRepository.GetByIdAsync(session.SlotId.Value);
                if (oldSlot != null)
                {
                    oldSlot.Status = SlotStatus.Available;
                    _parkingSlotRepository.Update(oldSlot);
                }
            }

            newSlot.Status = SlotStatus.Occupied;
            _parkingSlotRepository.Update(newSlot);
            session.SlotId = newSlot.Id;
            session.ZoneId = newSlot.ZoneId;
        }
        else if (request.ZoneId.HasValue)
        {
            // Allow changing zone without changing slot
            session.ZoneId = request.ZoneId.Value;
            session.SlotId = null;
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();

        // Reload with details
        var updated = await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
        return BaseResponse<ParkingSessionDto>.Ok(Map(updated ?? session), "Check-in info updated successfully.");
    }

    public async Task<BaseResponse<CheckInBookingLookupDto>> GetCheckInBookingByLicensePlateAsync(string licensePlate, int? buildingId = null)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return BaseResponse<CheckInBookingLookupDto>.Fail("INVALID_LICENSE_PLATE", "License plate is required.");
        }

        var normalizedPlate = Normalize(licensePlate);
        var booking = await _sessionRepository.GetActiveBookingForCheckInByLicensePlateAsync(normalizedPlate, buildingId);
        if (booking == null)
        {
            return BaseResponse<CheckInBookingLookupDto>.Fail("BOOKING_NOT_FOUND", "No confirmed booking found for this license plate.");
        }

        return BaseResponse<CheckInBookingLookupDto>.Ok(new CheckInBookingLookupDto
        {
            BookingId = booking.Id,
            BookingCode = FormatBookingCode(booking.Id),
            LicensePlate = booking.Vehicle.LicensePlate,
            VehicleTypeId = booking.VehicleTypeId,
            VehicleTypeName = booking.VehicleType?.TypeName,
            BuildingId = booking.BuildingId,
            BuildingName = booking.Building?.Name,
            PlannedCheckinTime = booking.PlannedCheckinTime,
            CheckinGraceUntil = booking.CheckinGraceUntil,
            BookingStatus = booking.BookingStatus
        });
    }

    public async Task<BaseResponse<ParkingSessionDto>> CreateAsync(CreateParkingSessionRequest request)
    {


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
            MonthlySubscriptionId = null,
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
        var sessions = await _sessionRepository.GetActiveSessionsWithDetailsAsync();
        return BaseResponse<IEnumerable<ParkingSessionDto>>.Ok(sessions.Select(Map).ToList());
    }

    public async Task<BaseResponse<IEnumerable<ParkingSessionDto>>> GetByAccountIdAsync(int accountId)
    {
        var sessions = await _sessionRepository.GetByAccountIdAsync(accountId);
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
        var session = await _sessionRepository.GetSessionWithDetailsAsync(id);
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

        ParkingSlot? newSlot = null;
        if (request.SlotId.HasValue)
        {
            newSlot = await _parkingSlotRepository.GetSlotWithDetailsAsync(request.SlotId.Value);
            if (newSlot == null)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_FOUND", $"Parking slot with ID {request.SlotId.Value} not found.");
            }
            if (newSlot.Status is SlotStatus.Blocked or SlotStatus.Maintenance or SlotStatus.Reserved)
            {
                return BaseResponse<ParkingSessionDto>.Fail("SLOT_NOT_AVAILABLE", "Selected slot is currently blocked, under maintenance, or reserved.");
            }

            // Kiểm tra khớp loại phương tiện (Vehicle Type)
            if (session.Vehicle != null && newSlot.VehicleTypeId != session.Vehicle.VehicleTypeId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_TYPE_MISMATCH", "Selected slot does not match the vehicle type of this session.");
            }

            // Kiểm tra khớp tòa nhà (Building)
            if (newSlot.Zone?.Floor != null && newSlot.Zone.Floor.BuildingId != session.BuildingId)
            {
                return BaseResponse<ParkingSessionDto>.Fail("BUILDING_MISMATCH", "Selected slot is located in a different building.");
            }
        }

        var oldSlotId = session.SlotId;

        // Giải phóng slot cũ và chiếm dụng slot mới nếu khác nhau
        if (oldSlotId != request.SlotId)
        {
            if (oldSlotId.HasValue)
            {
                var oldSlot = await _parkingSlotRepository.GetByIdAsync(oldSlotId.Value);
                if (oldSlot != null)
                {
                    oldSlot.Status = SlotStatus.Available;
                    _parkingSlotRepository.Update(oldSlot);
                }
            }

            if (newSlot != null)
            {
                newSlot.Status = SlotStatus.Occupied;
                _parkingSlotRepository.Update(newSlot);
            }
        }

        if (newSlot != null)
        {
            session.ZoneId = newSlot.ZoneId;
            session.SlotId = newSlot.Id;
        }
        else
        {
            if (request.ZoneId.HasValue)
            {
                session.ZoneId = request.ZoneId.Value;
            }
            session.SlotId = null;
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Assigned parking slot successfully.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> StartCheckoutAsync(int id, StartCheckoutRequest request)
    {
        var session = await _sessionRepository.GetSessionWithDetailsAsync(id);
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
        session.ImageOut = request.ImageOut;

        // Tạo sự cố LATE_CHECKOUT nếu đỗ xe quá giờ (Booking)
        if (session.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(session.BookingId.Value);
            if (booking != null && checkOutTime > booking.PlannedCheckoutTime.AddMinutes(LateCheckoutGracePeriodMinutes))
            {
                var lateCheckoutType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "LATE_CHECKOUT");
                if (lateCheckoutType != null)
                {
                    var openIncidents = await _incidentRepository.FindAsync(i => i.SessionId == session.Id && i.IncidentTypeId == lateCheckoutType.Id);
                    if (!openIncidents.Any())
                    {
                        var activePenalty = await _penaltyConfigRepository.FirstOrDefaultAsync(pc => pc.IncidentTypeId == lateCheckoutType.Id && pc.IsActive && !pc.IsDeleted);
                        decimal penaltyFee = activePenalty?.PenaltyFee ?? 50000;

                        var incident = new IncidentEntity
                        {
                            SessionId = session.Id,
                            IncidentTypeId = lateCheckoutType.Id,
                            PenaltyConfigId = activePenalty?.Id,
                            PenaltyFee = penaltyFee,
                            Status = IncidentStatus.Open,
                            Description = $"Đỗ xe quá giờ (Check-out muộn: {checkOutTime}, Planned: {booking.PlannedCheckoutTime})"
                        };
                        await _incidentRepository.AddAsync(incident);
                        await _incidentRepository.SaveChangesAsync();
                    }
                }
            }
        }

        // Tính toán phí gửi xe và phí phạt gộp chung qua PricingEngine
        decimal totalFee = 0;
        decimal totalPenaltyFee = 0;
        decimal amountDue = 0;
        var calculationStartTime = session.CheckInTime;
 
        var vehicle = session.Vehicle ?? await _vehicleRepository.GetByIdAsync(session.VehicleId);
        if (vehicle == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("VEHICLE_NOT_FOUND", $"Vehicle with ID {session.VehicleId} not found.");
        }
 

 
        if (calculationStartTime < checkOutTime)
        {
            // PricingEngine sẽ tự tính phí đỗ (bao gồm block/lũy tiến/giá trần) cộng phí phạt các sự cố Open
            var feeResult = await _pricingCalculationService.CalculateFeeAsync(vehicle.VehicleTypeId, calculationStartTime, checkOutTime, session.Id);
            totalFee = feeResult.BaseAmount + feeResult.IncrementAmount;
            totalPenaltyFee = feeResult.PenaltyAmount;
            amountDue = feeResult.TotalAmount;
        }
        else
        {
            // Lấy tổng tiền phạt từ các sự cố Open hiện có (nếu thời gian gửi không tốn phí đỗ xe)
            var sessionIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(session.Id);
            if (sessionIncidents != null)
            {
                var openIncidents = sessionIncidents.Where(i => i.Status == IncidentStatus.Open && !i.IsDeleted);
                totalPenaltyFee = openIncidents.Sum(i => i.PenaltyFee ?? 0);
            }
            amountDue = totalPenaltyFee;
        }
 
        // Nếu là Booking, khấu trừ tiền đặt cọc vào phần tiền cần thanh toán cuối cùng
        if (session.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(session.BookingId.Value);
            if (booking != null)
            {
                // Khấu trừ đặt cọc trước
                amountDue = Math.Max(0, amountDue - booking.DepositAmount);
                // Cập nhật lại totalFee sau khấu trừ (phần hiển thị phí đỗ xe nếu cần)
                totalFee = Math.Max(0, totalFee - booking.DepositAmount);
            }
        }
 
        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
 
        var dto = Map(session);
        dto.TotalFee = totalFee;
        dto.PenaltyFee = totalPenaltyFee;
        dto.AmountDue = amountDue;
 
        return BaseResponse<ParkingSessionDto>.Ok(dto, "Started checkout successfully. Waiting for completion.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> CompleteAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {id} not found.");
        }

        if (session.SessionStatus == CompletedStatus)
        {
            return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Parking session is already completed.");
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

        // Tự động giải quyết tất cả sự cố chưa hoàn thành (Open / Processing) của session sau khi thanh toán/hoàn tất thành công
        var sessionIncidents = await _incidentRepository.GetIncidentsBySessionWithDetailsAsync(id);
        if (sessionIncidents != null)
        {
            var activeIncidents = sessionIncidents.Where(i => 
                (i.Status == IncidentStatus.Open || i.Status == IncidentStatus.Processing) && !i.IsDeleted);

            foreach (var incident in activeIncidents)
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

        // Kiểm tra xem đã có giao dịch PAID nào chưa
        var hasPaidPayment = await _sessionRepository.HasPaidPaymentForSessionAsync(id);
        if (hasPaidPayment)
        {
            return BaseResponse<ParkingSessionDto>.Fail("PAYMENT_ALREADY_PROCESSED", "Cannot rollback checkout because a successful payment has already been processed for this session.");
        }

        session.CheckOutTime = null;
        session.LicensePlateOut = null;
        session.OutStaffId = null;

        // Xóa sự cố đỗ quá giờ nếu có khi rollback check-out
        var lateCheckoutType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "LATE_CHECKOUT");
        if (lateCheckoutType != null)
        {
            var lateIncidents = await _incidentRepository.FindAsync(i => i.SessionId == session.Id && i.IncidentTypeId == lateCheckoutType.Id);
            foreach (var incident in lateIncidents)
            {
                await _incidentRepository.RemoveAsync(incident);
            }
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();
        return BaseResponse<ParkingSessionDto>.Ok(Map(session), "Rolled back checkout successfully.");
    }

    private static bool IsActive(ParkingSessionEntity session) =>
        string.Equals(session.SessionStatus, ActiveStatus, StringComparison.OrdinalIgnoreCase);

    private static bool StatusEquals(string value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static bool IsCar(VehicleTypeEntity vehicleType) =>
        !string.IsNullOrWhiteSpace(vehicleType.TypeName) && (
        string.Equals(vehicleType.TypeName, VehicleTypeEntity.CarTypeName, StringComparison.OrdinalIgnoreCase) ||
        vehicleType.TypeName.Contains("CAR", StringComparison.OrdinalIgnoreCase) ||
        vehicleType.TypeName.Contains("AUTO", StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string FormatBookingCode(int bookingId) => $"BK-{bookingId:D6}";

    public async Task SendOvertimeWarningsAsync()
    {
        var now = DateTime.UtcNow;
        var warningTimeLimit = now.AddMinutes(15);

        var sessionsToWarn = await _sessionRepository.GetOvertimeWarningSessionsAsync(warningTimeLimit, now);

        foreach (var session in sessionsToWarn)
        {
            var booking = session.Booking!;

            // Kiểm tra xem đã gửi thông báo cảnh báo cho booking này chưa
            var alreadyWarned = await _notificationRepository.AnyAsync(n =>
                n.AccountId == booking.AccountId &&
                n.Title == "Cảnh báo sắp hết giờ đỗ xe" &&
                n.CreatedAt >= booking.PlannedCheckinTime);

            if (!alreadyWarned)
            {
                var notification = new Notification
                {
                    AccountId = booking.AccountId,
                    Title = "Cảnh báo sắp hết giờ đỗ xe",
                    Message = $"Lượt gửi xe của phương tiện {session.Vehicle.LicensePlate} sắp hết giờ đăng ký (dự kiến ra: {booking.PlannedCheckoutTime:HH:mm dd/MM/yyyy}). Vui lòng di chuyển xe hoặc gia hạn.",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
            }
        }

        await _notificationRepository.SaveChangesAsync();
    }

    private static ParkingSessionDto Map(ParkingSessionEntity session) => new()
    {
        Id = session.Id,
        VehicleId = session.VehicleId,
        AccountId = session.Vehicle?.AccountId,
        BuildingId = session.BuildingId,
        CardId = session.CardId,
        ZoneId = session.ZoneId,
        SlotId = session.SlotId,
        BookingId = session.BookingId,
        BookingCode = session.BookingId.HasValue ? FormatBookingCode(session.BookingId.Value) : null,
        MonthlySubscriptionId = null,
        InStaffId = session.InStaffId,
        OutStaffId = session.OutStaffId,
        CheckInTime = session.CheckInTime,
        CheckOutTime = session.CheckOutTime,
        LicensePlateIn = session.LicensePlateIn,
        LicensePlateOut = session.LicensePlateOut,
        ImageIn = session.ImageIn,
        ImageOut = session.ImageOut,
        SessionStatus = session.SessionStatus,
        CardCode = session.Card?.CardCode,
        ZoneCode = session.Zone?.Code,
        SlotCode = session.ParkingSlot?.Code,
        VehicleType = session.Vehicle?.VehicleType?.TypeName,
        CustomerType = session.BookingId.HasValue ? "BOOKING" : "WALK_IN"
    };

    public async Task<BaseResponse<ParkingSessionDto>> ReplaceSessionCardAsync(int sessionId, string newCardCode)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new NotFoundException("ParkingSession", sessionId);
        }

        if (session.SessionStatus != ActiveStatus)
        {
            throw new DomainException("SESSION_NOT_ACTIVE", "Parking session is not active.");
        }

        var normalizedCardCode = newCardCode.Trim().ToUpper();
        var newCard = await _cardRepository.GetByCardCodeAsync(normalizedCardCode);
        if (newCard == null)
        {
            throw new NotFoundException("Card", newCardCode);
        }

        if (newCard.CardStatus != CardStatus.Available.ToString())
        {
            throw new DomainException("CARD_NOT_AVAILABLE", $"The replacement card '{newCardCode}' is not available (Status: {newCard.CardStatus}).");
        }

        // 1. Cập nhật thẻ cũ sang trạng thái LOST và đặt LostAt
        var oldCard = await _cardRepository.GetByIdAsync(session.CardId);
        if (oldCard != null)
        {
            oldCard.CardStatus = CardStatus.Lost.ToString();
            oldCard.LostAt = DateTime.UtcNow;
            _cardRepository.Update(oldCard);
        }

        // 2. Cập nhật thẻ mới sang trạng thái ACTIVE
        newCard.CardStatus = CardStatus.Active.ToString();
        _cardRepository.Update(newCard);

        // 3. Cập nhật session liên kết với thẻ mới
        session.CardId = newCard.Id;
        _sessionRepository.Update(session);

        // 4. Tự động báo cáo sự cố LOST_CARD nếu chưa có để hệ thống tính phí phạt khi checkout
        var lostCardType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "LOST_CARD");
        if (lostCardType != null)
        {
            var openLostIncidents = await _incidentRepository.FindAsync(i => i.SessionId == session.Id && i.IncidentTypeId == lostCardType.Id && i.Status == IncidentStatus.Open);
            if (!openLostIncidents.Any())
            {
                var activePenalty = await _penaltyConfigRepository.FirstOrDefaultAsync(pc => pc.IncidentTypeId == lostCardType.Id && pc.IsActive && !pc.IsDeleted);
                var incident = new IncidentEntity
                {
                    SessionId = session.Id,
                    IncidentTypeId = lostCardType.Id,
                    Description = $"Báo mất thẻ gửi xe (Thẻ cũ: {oldCard?.CardCode})",
                    Status = IncidentStatus.Open,
                    PenaltyFee = activePenalty?.PenaltyFee ?? 100000,
                    CreatedAt = DateTime.UtcNow
                };
                await _incidentRepository.AddAsync(incident);
            }
        }

        await _sessionRepository.SaveChangesAsync();

        // Load details for mapping
        var updatedSession = await _sessionRepository.GetSessionWithDetailsAsync(session.Id);
        return BaseResponse<ParkingSessionDto>.Ok(Map(updatedSession ?? session));
    }

    public async Task<BaseResponse<ParkingSessionDto>> UnpaidCheckoutAsync(int sessionId, UnpaidCheckoutRequest request)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {sessionId} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be checked out as unpaid.");
        }

        // 1. Cập nhật trạng thái Session thành UNPAID và thời gian out
        session.CheckOutTime = DateTime.UtcNow.AddHours(7);
        session.LicensePlateOut ??= session.LicensePlateIn;
        session.SessionStatus = "UNPAID";
        session.OutStaffId = request.StaffId;

        // 2. Giải phóng slot nếu có
        if (session.SlotId.HasValue)
        {
            var slot = await _parkingSlotRepository.GetByIdAsync(session.SlotId.Value);
            if (slot != null)
            {
                slot.Status = SlotStatus.Available;
                _parkingSlotRepository.Update(slot);
            }
        }

        // 3. Giải phóng Card (đưa Card về Available nếu không phải thẻ tháng)
        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card != null && card.CardStatus == CardStatus.Active.ToString())
        {
            card.CardStatus = CardStatus.Available.ToString();
            _cardRepository.Update(card);
        }

        // 4. Tạo sự cố UNPAID_VEHICLE
        var unpaidIncidentType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "UNPAID_VEHICLE");
        if (unpaidIncidentType == null)
        {
            // Tạo mới IncidentType nếu chưa có
            unpaidIncidentType = new IncidentType
            {
                IncidentCode = "UNPAID_VEHICLE",
                IncidentName = "Check-out chưa thanh toán",
                Description = "Phương tiện ra khỏi bãi đỗ xe nhưng chưa hoàn tất thanh toán"
            };
            await _incidentTypeRepository.AddAsync(unpaidIncidentType);
            await _incidentTypeRepository.SaveChangesAsync();
        }

        var incident = new IncidentEntity
        {
            SessionId = session.Id,
            IncidentTypeId = unpaidIncidentType.Id,
            Description = request.Reason,
            Status = IncidentStatus.Open, // Giữ Open vì khách chưa trả tiền
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };
        await _incidentRepository.AddAsync(incident);
        await _incidentRepository.SaveChangesAsync();

        // 5. Đưa xe vào Blacklist
        var existingBlacklist = await _blacklistRepository.AnyAsync(b => b.VehicleId == session.VehicleId && !b.IsDeleted);
        if (!existingBlacklist)
        {
            var blacklist = new PBMS.Domain.Entities.Blacklist
            {
                VehicleId = session.VehicleId,
                CardId = card?.Id,
                IncidentId = incident.Id,
                Reason = $"Quỵt tiền/Chưa thanh toán cho phiên gửi xe #{session.Id}. Lý do: {request.Reason}"
            };
            await _blacklistRepository.AddAsync(blacklist);
        }

        _sessionRepository.Update(session);
        await _sessionRepository.SaveChangesAsync();

        // 6. Gửi thông báo cho toàn bộ Managers trong hệ thống về việc xe quỵt tiền
        try
        {
            var allAccounts = await _accountRepository.GetAllWithRolesAsync();
            var managers = allAccounts.Where(a => string.Equals(a.Role?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase));

            foreach (var manager in managers)
            {
                var notification = new Notification
                {
                    AccountId = manager.Id,
                    Title = "Xe quỵt tiền thanh toán check-out",
                    Message = $"Xe biển số {session.LicensePlateIn} thực hiện check-out không thanh toán lúc {session.CheckOutTime:dd/MM/yyyy HH:mm:ss} và đã bị đưa vào Blacklist.",
                    CreatedAt = DateTime.UtcNow.AddHours(7)
                };
                await _notificationRepository.AddAsync(notification);
            }
            await _notificationRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Bỏ qua lỗi gửi thông báo để tránh rollback transaction chính
        }

        var updatedSession = await _sessionRepository.GetSessionWithDetailsAsync(session.Id);
        return BaseResponse<ParkingSessionDto>.Ok(Map(updatedSession ?? session), "Checked out as unpaid successfully. Vehicle has been blacklisted.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> ReportLostCardAsync(int sessionId, LostCardRequest request)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {sessionId} not found.");
        }

        if (!IsActive(session))
        {
            return BaseResponse<ParkingSessionDto>.Fail("SESSION_NOT_ACTIVE", "Only active sessions can be reported as lost card.");
        }

        // 1. Tìm thẻ và chuyển sang trạng thái Lost
        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Card linked to session {sessionId} not found.");
        }

        card.CardStatus = CardStatus.Lost.ToString();
        card.LostAt = DateTime.UtcNow.AddHours(7);
        _cardRepository.Update(card);

        // 2. Tạo sự cố LOST_CARD
        var lostCardType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "LOST_CARD");
        if (lostCardType == null)
        {
            lostCardType = new IncidentType
            {
                IncidentCode = "LOST_CARD",
                IncidentName = "Mất thẻ gửi xe",
                Description = "Khách hàng báo mất thẻ tại cổng ra hoặc trong bãi"
            };
            await _incidentTypeRepository.AddAsync(lostCardType);
            await _incidentTypeRepository.SaveChangesAsync();
        }

        var openLostIncidents = await _incidentRepository.FindAsync(i => i.SessionId == session.Id && i.IncidentTypeId == lostCardType.Id && i.Status == IncidentStatus.Open);
        if (!openLostIncidents.Any())
        {
            var activePenalty = await _penaltyConfigRepository.FirstOrDefaultAsync(pc => pc.IncidentTypeId == lostCardType.Id && pc.IsActive && !pc.IsDeleted);
            var incident = new IncidentEntity
            {
                SessionId = session.Id,
                IncidentTypeId = lostCardType.Id,
                Description = request.Description,
                Status = IncidentStatus.Open,
                PenaltyFee = activePenalty?.PenaltyFee ?? 100000,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };
            await _incidentRepository.AddAsync(incident);
        }

        await _sessionRepository.SaveChangesAsync();

        var updatedSession = await _sessionRepository.GetSessionWithDetailsAsync(session.Id);
        return BaseResponse<ParkingSessionDto>.Ok(Map(updatedSession ?? session), "Reported lost card successfully. Card status set to Lost and incident penalty applied.");
    }

    public async Task<BaseResponse<ParkingSessionDto>> RollbackLostCardAsync(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Parking session with ID {sessionId} not found.");
        }

        var card = await _cardRepository.GetByIdAsync(session.CardId);
        if (card == null)
        {
            return BaseResponse<ParkingSessionDto>.Fail("NOT_FOUND", $"Card linked to session {sessionId} not found.");
        }

        // 1. Tìm các incident LOST_CARD của session này
        var lostCardType = await _incidentTypeRepository.FirstOrDefaultAsync(it => it.IncidentCode == "LOST_CARD");
        if (lostCardType != null)
        {
            var activeIncidents = await _incidentRepository.FindAsync(i => 
                i.SessionId == session.Id && 
                i.IncidentTypeId == lostCardType.Id && 
                (i.Status == IncidentStatus.Open || i.Status == IncidentStatus.Processing) && 
                !i.IsDeleted);

            foreach (var incident in activeIncidents)
            {
                // Chuyển incident sang Cancelled
                incident.Status = IncidentStatus.Cancelled;
                _incidentRepository.Update(incident);

                // Gỡ Blacklist liên kết với incident này (nếu có)
                var incidentBlacklists = await _blacklistRepository.FindAsync(b => b.IncidentId == incident.Id && !b.IsDeleted);
                foreach (var b in incidentBlacklists)
                {
                    await _blacklistRepository.RemoveAsync(b);
                }
            }
        }

        // 2. Gỡ bỏ Blacklist trực tiếp của Xe và Thẻ liên quan
        var vehicleBlacklists = await _blacklistRepository.FindAsync(b => b.VehicleId == session.VehicleId && !b.IsDeleted);
        foreach (var b in vehicleBlacklists)
        {
            await _blacklistRepository.RemoveAsync(b);
        }

        var cardBlacklists = await _blacklistRepository.FindAsync(b => b.CardId == card.Id && !b.IsDeleted);
        foreach (var b in cardBlacklists)
        {
            await _blacklistRepository.RemoveAsync(b);
        }

        // 3. Khôi phục trạng thái Thẻ
        card.CardStatus = CardStatus.Active.ToString(); // Vì session đỗ xe này vẫn đang diễn ra
        card.LostAt = null;
        _cardRepository.Update(card);

        await _sessionRepository.SaveChangesAsync();

        var updatedSession = await _sessionRepository.GetSessionWithDetailsAsync(session.Id);
        return BaseResponse<ParkingSessionDto>.Ok(Map(updatedSession ?? session), "Rollback lost card successfully. Card status restored and blacklist blocks removed.");
    }
}

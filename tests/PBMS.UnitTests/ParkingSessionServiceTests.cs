using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSession.DTOs;
using PBMS.Application.ParkingSession.Services;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;

namespace PBMS.UnitTests;

public class ParkingSessionServiceTests
{
    private readonly IParkingSessionRepository _sessionRepositoryMock;
    private readonly IRepository<VehicleEntity> _vehicleRepositoryMock;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepositoryMock;
    private readonly IRepository<Booking> _bookingRepositoryMock;
    private readonly IFeeCalculationService _feeCalculationServiceMock;
    private readonly ICardRepository _cardRepositoryMock;
    private readonly IMonthlySubscriptionRepository _subscriptionRepositoryMock;
    private readonly IParkingSlotRepository _parkingSlotRepositoryMock;
    private readonly IIncidentRepository _incidentRepositoryMock;
    private readonly IRepository<IncidentType> _incidentTypeRepositoryMock;
    private readonly IRepository<PenaltyConfig> _penaltyConfigRepositoryMock;
    private readonly IBlacklistRepository _blacklistRepositoryMock;
    private readonly IRepository<Notification> _notificationRepositoryMock;
    private readonly ParkingSessionService _service;

    public ParkingSessionServiceTests()
    {
        _sessionRepositoryMock = Substitute.For<IParkingSessionRepository>();
        _vehicleRepositoryMock = Substitute.For<IRepository<VehicleEntity>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleTypeEntity>>();
        _bookingRepositoryMock = Substitute.For<IRepository<Booking>>();
        _feeCalculationServiceMock = Substitute.For<IFeeCalculationService>();
        _cardRepositoryMock = Substitute.For<ICardRepository>();
        _subscriptionRepositoryMock = Substitute.For<IMonthlySubscriptionRepository>();
        _parkingSlotRepositoryMock = Substitute.For<IParkingSlotRepository>();
        _incidentRepositoryMock = Substitute.For<IIncidentRepository>();
        _incidentTypeRepositoryMock = Substitute.For<IRepository<IncidentType>>();
        _penaltyConfigRepositoryMock = Substitute.For<IRepository<PenaltyConfig>>();
        _blacklistRepositoryMock = Substitute.For<IBlacklistRepository>();
        _notificationRepositoryMock = Substitute.For<IRepository<Notification>>();

        _service = new ParkingSessionService(
            _sessionRepositoryMock,
            _vehicleRepositoryMock,
            _vehicleTypeRepositoryMock,
            _bookingRepositoryMock,
            _feeCalculationServiceMock,
            _cardRepositoryMock,
            _subscriptionRepositoryMock,
            _parkingSlotRepositoryMock,
            _incidentRepositoryMock,
            _incidentTypeRepositoryMock,
            _penaltyConfigRepositoryMock,
            _blacklistRepositoryMock,
            _notificationRepositoryMock
        );
    }

    [Fact]
    public async Task CheckInAsync_ShouldBypassAvailableCheck_WhenCardIsAssignedToActiveSubscription()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "M-CARD-1",
            VehicleTypeId = 1,
            BuildingId = 10,
            StaffId = 5
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = VehicleTypeEntity.MotorcycleTypeName };
        var card = new Card { Id = 100, CardCode = "M-CARD-1", CardType = "MONTHLY", CardStatus = CardStatus.Assigned.ToString() };
        var vehicle = new VehicleEntity { Id = 200, LicensePlate = "29A-12345", VehicleTypeId = 1 };
        var activeSub = new MonthlySubscription
        {
            Id = 500,
            VehicleId = 200,
            BuildingId = 10,
            AssignedCardId = 100,
            ActivatedAt = DateTime.UtcNow.AddDays(-5),
            ExpiredAt = DateTime.UtcNow.AddDays(25),
            Vehicle = vehicle,
            MonthlySubscriptionStatus = "ACTIVE"
        };
        var zone = new Zone { Id = 9, Code = "M-ZONE", Floor = new Floor { BuildingId = 10 } };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("M-CARD-1").Returns(card);
        _subscriptionRepositoryMock.GetActiveSubscriptionByCardIdAsync(100).Returns(activeSub);
        _sessionRepositoryMock.GetVehicleByLicensePlateAsync("29A-12345").Returns(vehicle);
        _sessionRepositoryMock.HasActiveSessionForVehicleAsync(200).Returns(false);
        _sessionRepositoryMock.FindAvailableZoneAsync(1, 10).Returns(zone);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(500, result.Data.MonthlySubscriptionId);
        Assert.Equal("Assigned", card.CardStatus); // Trạng thái thẻ tháng giữ nguyên Assigned
    }

    [Fact]
    public async Task StartCheckoutAsync_ShouldNotCompleteImmediately_WhenMonthlySubscriptionIsValid_DueToTask4Refactoring()
    {
        // Arrange
        int sessionId = 1;
        var request = new StartCheckoutRequest { CheckOutTime = DateTime.UtcNow };

        var session = new ParkingSession
        {
            Id = sessionId,
            VehicleId = 2,
            CardId = 3,
            SessionStatus = "ACTIVE",
            CheckInTime = DateTime.UtcNow.AddHours(-2),
            MonthlySubscriptionId = 500,
            Vehicle = new Vehicle { Id = 2, VehicleTypeId = 1 }
        };

        var subscription = new MonthlySubscription
        {
            Id = 500,
            ExpiredAt = DateTime.UtcNow.AddDays(10) // Còn hạn
        };

        _sessionRepositoryMock.GetSessionWithDetailsAsync(sessionId).Returns(session);
        _subscriptionRepositoryMock.GetByIdAsync(500).Returns(subscription);

        // Act
        var result = await _service.StartCheckoutAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ACTIVE", result.Data.SessionStatus); // Giữ ACTIVE theo yêu cầu Task 4
        Assert.Equal(0, result.Data.TotalFee);
        Assert.Equal(0, result.Data.AmountDue);
    }

    [Fact]
    public async Task StartCheckoutAsync_ShouldNotCompleteImmediately_WhenMonthlySubscriptionIsExpired()
    {
        // Arrange
        int sessionId = 1;
        var checkOutTime = DateTime.UtcNow;
        var request = new StartCheckoutRequest { CheckOutTime = checkOutTime };

        var session = new ParkingSession
        {
            Id = sessionId,
            VehicleId = 2,
            CardId = 3,
            SessionStatus = "ACTIVE",
            CheckInTime = DateTime.UtcNow.AddHours(-10),
            MonthlySubscriptionId = 500,
            Vehicle = new Vehicle { Id = 2, VehicleTypeId = 1 }
        };

        var expiredAt = DateTime.UtcNow.AddHours(-5); // Hết hạn trước lúc check-out
        var subscription = new MonthlySubscription
        {
            Id = 500,
            ExpiredAt = expiredAt
        };

        _sessionRepositoryMock.GetSessionWithDetailsAsync(sessionId).Returns(session);
        _subscriptionRepositoryMock.GetByIdAsync(500).Returns(subscription);
        _feeCalculationServiceMock.CalculateFeeAsync(1, expiredAt, checkOutTime)
            .Returns(new FeeCalculationResult { TotalFee = 15000 });

        // Act
        var result = await _service.StartCheckoutAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("ACTIVE", result.Data.SessionStatus); // Giữ ACTIVE chờ thanh toán phí overtime
        Assert.Equal(15000, result.Data.TotalFee);
        Assert.Equal(15000, result.Data.AmountDue);
    }

    [Fact]
    public async Task CheckInAsync_ShouldAutoLinkBooking_WhenActiveConfirmedBookingExists()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "CARD-999",
            VehicleTypeId = 1,
            BuildingId = 10,
            StaffId = 5
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = VehicleTypeEntity.MotorcycleTypeName };
        var card = new Card { Id = 100, CardCode = "CARD-999", CardType = "NORMAL", CardStatus = CardStatus.Available.ToString() };
        var vehicle = new VehicleEntity { Id = 200, LicensePlate = "29A-12345", VehicleTypeId = 1 };
        var zone = new Zone { Id = 9, Code = "M-ZONE", Floor = new Floor { BuildingId = 10 } };

        var booking = new Booking
        {
            Id = 700,
            VehicleId = 200,
            BuildingId = 10,
            BookingStatus = BookingStatus.Confirmed,
            PlannedCheckinTime = DateTime.UtcNow,
            CheckinGraceUntil = DateTime.UtcNow.AddMinutes(30),
            Vehicle = vehicle
        };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("CARD-999").Returns(card);
        _sessionRepositoryMock.GetVehicleByLicensePlateAsync("29A-12345").Returns(vehicle);
        _sessionRepositoryMock.HasActiveSessionForVehicleAsync(200).Returns(false);
        _sessionRepositoryMock.FindAvailableZoneAsync(1, 10).Returns(zone);

        // Mock FirstOrDefaultAsync trên _bookingRepositoryMock
        _bookingRepositoryMock.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Booking, bool>>>())
            .Returns(booking);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(700, result.Data.BookingId); // Phải liên kết BookingId tự động
        Assert.Equal(BookingStatus.CheckedIn, booking.BookingStatus); // Trạng thái Booking phải chuyển sang CheckedIn
        _bookingRepositoryMock.Received(1).Update(booking); // Phải lưu Booking cập nhật
    }

    [Fact]
    public async Task AssignSlotAsync_ShouldReleaseOldSlotAndOccupyNewSlot_WhenSlotsAreDifferent()
    {
        // Arrange
        int sessionId = 1;
        var request = new AssignParkingSessionSlotRequest { ZoneId = 1, SlotId = 20 };
        
        var session = new ParkingSession
        {
            Id = sessionId,
            SessionStatus = "ACTIVE",
            SlotId = 10, // Old slot ID
            ZoneId = 1,
            BuildingId = 1,
            Vehicle = new Vehicle { VehicleTypeId = 1 }
        };

        var oldSlot = new ParkingSlot { Id = 10, Status = SlotStatus.Occupied };
        var newSlot = new ParkingSlot 
        { 
            Id = 20, 
            Status = SlotStatus.Available, 
            ZoneId = 1,
            VehicleTypeId = 1,
            Zone = new Zone { Id = 1, Floor = new Floor { BuildingId = 1 } }
        };

        _sessionRepositoryMock.GetSessionWithDetailsAsync(sessionId).Returns(session);
        _sessionRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ParkingSession, bool>>>()).Returns(false);
        _parkingSlotRepositoryMock.GetSlotWithDetailsAsync(20).Returns(newSlot);
        _parkingSlotRepositoryMock.GetByIdAsync(10).Returns(oldSlot);

        // Act
        var result = await _service.AssignSlotAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(20, session.SlotId);
        Assert.Equal(SlotStatus.Available, oldSlot.Status);
        Assert.Equal(SlotStatus.Occupied, newSlot.Status);
        _parkingSlotRepositoryMock.Received(1).Update(oldSlot);
        _parkingSlotRepositoryMock.Received(1).Update(newSlot);
        _sessionRepositoryMock.Received(1).Update(session);
    }

    [Fact]
    public async Task AssignSlotAsync_ShouldFail_WhenVehicleTypeMismatches()
    {
        // Arrange
        int sessionId = 1;
        var request = new AssignParkingSessionSlotRequest { ZoneId = 1, SlotId = 20 };
        
        var session = new ParkingSession
        {
            Id = sessionId,
            SessionStatus = "ACTIVE",
            SlotId = 10,
            ZoneId = 1,
            BuildingId = 1,
            Vehicle = new Vehicle { VehicleTypeId = 1 }
        };

        var newSlot = new ParkingSlot 
        { 
            Id = 20, 
            Status = SlotStatus.Available, 
            ZoneId = 1,
            VehicleTypeId = 2, // Mismatch
            Zone = new Zone { Id = 1, Floor = new Floor { BuildingId = 1 } }
        };

        _sessionRepositoryMock.GetSessionWithDetailsAsync(sessionId).Returns(session);
        _sessionRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ParkingSession, bool>>>()).Returns(false);
        _parkingSlotRepositoryMock.GetSlotWithDetailsAsync(20).Returns(newSlot);

        // Act
        var result = await _service.AssignSlotAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("VEHICLE_TYPE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task AssignSlotAsync_ShouldFail_WhenBuildingMismatches()
    {
        // Arrange
        int sessionId = 1;
        var request = new AssignParkingSessionSlotRequest { ZoneId = 1, SlotId = 20 };
        
        var session = new ParkingSession
        {
            Id = sessionId,
            SessionStatus = "ACTIVE",
            SlotId = 10,
            ZoneId = 1,
            BuildingId = 1,
            Vehicle = new Vehicle { VehicleTypeId = 1 }
        };

        var newSlot = new ParkingSlot 
        { 
            Id = 20, 
            Status = SlotStatus.Available, 
            ZoneId = 1,
            VehicleTypeId = 1,
            Zone = new Zone { Id = 1, Floor = new Floor { BuildingId = 2 } } // Mismatch (Building 2)
        };

        _sessionRepositoryMock.GetSessionWithDetailsAsync(sessionId).Returns(session);
        _sessionRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ParkingSession, bool>>>()).Returns(false);
        _parkingSlotRepositoryMock.GetSlotWithDetailsAsync(20).Returns(newSlot);

        // Act
        var result = await _service.AssignSlotAsync(sessionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("BUILDING_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task CheckInAsync_ShouldFail_WhenCardIsBlacklisted()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "B-CARD-1",
            VehicleTypeId = 1,
            BuildingId = 1
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car" };
        var card = new Card { Id = 100, CardCode = "B-CARD-1", CardType = "NORMAL" };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("B-CARD-1").Returns(card);
        
        // Mock card being in blacklist
        _blacklistRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PBMS.Domain.Entities.Blacklist, bool>>>())
            .Returns(true);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("CARD_BLACKLISTED", result.ErrorCode);
    }

    [Fact]
    public async Task CheckInAsync_ShouldFail_WhenVehicleIsBlacklisted()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "G-CARD-1",
            VehicleTypeId = 1,
            BuildingId = 1
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car" };
        var card = new Card { Id = 100, CardCode = "G-CARD-1", CardType = "NORMAL", CardStatus = CardStatus.Available.ToString() };
        var vehicle = new VehicleEntity { Id = 200, LicensePlate = "29A-12345", VehicleTypeId = 1 };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("G-CARD-1").Returns(card);
        _sessionRepositoryMock.GetVehicleByLicensePlateAsync("29A-12345").Returns(vehicle);
        _sessionRepositoryMock.HasActiveSessionForVehicleAsync(200).Returns(false);

        // First call for CardId check returns false, second call for VehicleId check returns true
        _blacklistRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PBMS.Domain.Entities.Blacklist, bool>>>())
            .Returns(false, true);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("VEHICLE_BLACKLISTED", result.ErrorCode);
    }

    [Fact]
    public async Task CheckInAsync_ShouldFail_WhenCardIsAlreadyInActiveSession()
    {
        // Arrange
        var request = new CheckInRequest
        {
            LicensePlate = "29A-12345",
            CardCode = "A-CARD-1",
            VehicleTypeId = 1,
            BuildingId = 1
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car" };
        var card = new Card { Id = 100, CardCode = "A-CARD-1", CardType = "NORMAL" };

        _vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
        _cardRepositoryMock.GetByCardCodeAsync("A-CARD-1").Returns(card);
        
        // Mock card not blacklisted
        _blacklistRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PBMS.Domain.Entities.Blacklist, bool>>>())
            .Returns(false);
            
        // Mock card already in active session
        _sessionRepositoryMock.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ParkingSession, bool>>>())
            .Returns(true);

        // Act
        var result = await _service.CheckInAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("CARD_IN_ACTIVE_SESSION", result.ErrorCode);
    }

    [Fact]
    public async Task RollbackCheckoutAsync_ShouldSucceed_WhenNoPaidPaymentExists()
    {
        // Arrange
        int sessionId = 1;
        var session = new ParkingSession
        {
            Id = sessionId,
            SessionStatus = "ACTIVE",
            CheckOutTime = DateTime.UtcNow,
            LicensePlateOut = "29A-12345",
            OutStaffId = 10
        };

        _sessionRepositoryMock.GetByIdAsync(sessionId).Returns(session);
        _sessionRepositoryMock.HasPaidPaymentForSessionAsync(sessionId).Returns(false);
        _incidentTypeRepositoryMock.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<IncidentType, bool>>>())
            .Returns((IncidentType)null!);

        // Act
        var result = await _service.RollbackCheckoutAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(session.CheckOutTime);
        Assert.Null(session.LicensePlateOut);
        Assert.Null(session.OutStaffId);
        _sessionRepositoryMock.Received(1).Update(session);
    }

    [Fact]
    public async Task RollbackCheckoutAsync_ShouldFail_WhenPaidPaymentExists()
    {
        // Arrange
        int sessionId = 1;
        var session = new ParkingSession
        {
            Id = sessionId,
            SessionStatus = "ACTIVE",
            CheckOutTime = DateTime.UtcNow,
            LicensePlateOut = "29A-12345",
            OutStaffId = 10
        };

        _sessionRepositoryMock.GetByIdAsync(sessionId).Returns(session);
        _sessionRepositoryMock.HasPaidPaymentForSessionAsync(sessionId).Returns(true); // Đã có giao dịch PAID

        // Act
        var result = await _service.RollbackCheckoutAsync(sessionId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("PAYMENT_ALREADY_PROCESSED", result.ErrorCode);
    }

    [Fact]
    public async Task SendOvertimeWarningsAsync_ShouldAddNotification_WhenActiveSessionIsNearingPlannedCheckoutAndNotAlreadyWarned()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var warningTimeLimit = now.AddMinutes(15);

        var booking = new Booking
        {
            AccountId = 10,
            PlannedCheckinTime = now.AddHours(-1),
            PlannedCheckoutTime = now.AddMinutes(10)
        };

        var vehicle = new Vehicle { LicensePlate = "29A-12345" };

        var session = new PBMS.Domain.Entities.ParkingSession
        {
            Id = 1,
            SessionStatus = "ACTIVE",
            BookingId = 5,
            Booking = booking,
            Vehicle = vehicle
        };

        var sessionsToWarn = new List<PBMS.Domain.Entities.ParkingSession> { session };

        _sessionRepositoryMock.GetOvertimeWarningSessionsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(sessionsToWarn);

        _notificationRepositoryMock.AnyAsync(Arg.Any<Expression<Func<Notification, bool>>>())
            .Returns(false);

        // Act
        await _service.SendOvertimeWarningsAsync();

        // Assert
        await _notificationRepositoryMock.Received(1).AddAsync(Arg.Any<Notification>());
        await _notificationRepositoryMock.Received(1).SaveChangesAsync();
    }
}


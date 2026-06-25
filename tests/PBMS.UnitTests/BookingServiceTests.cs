using NSubstitute;
using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Services;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using BookingEntity = PBMS.Domain.Entities.Booking;
using VehicleEntity = PBMS.Domain.Entities.Vehicle;
using VehicleTypeEntity = PBMS.Domain.Entities.VehicleType;
using BuildingEntity = PBMS.Domain.Entities.Building;
using ParkingSessionEntity = PBMS.Domain.Entities.ParkingSession;
using ParkingSlotEntity = PBMS.Domain.Entities.ParkingSlot;
using PaymentEntity = PBMS.Domain.Entities.Payment;

using PBMS.Application.Pricing.Interfaces;

namespace PBMS.UnitTests;

public class BookingServiceTests
{
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly IRepository<VehicleEntity> _vehicleRepositoryMock;
    private readonly IRepository<VehicleTypeEntity> _vehicleTypeRepositoryMock;
    private readonly IRepository<BuildingEntity> _buildingRepositoryMock;
    private readonly IBuildingRepository _buildingDetailRepositoryMock;
    private readonly IPricingPolicyRepository _pricingPolicyRepositoryMock;
    private readonly IRepository<ParkingSessionEntity> _sessionRepositoryMock;
    private readonly IRepository<ParkingSlotEntity> _parkingSlotRepositoryMock;
    private readonly IRepository<PaymentEntity> _paymentRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IFeeCalculationService _feeCalculationServiceMock;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _vehicleRepositoryMock = Substitute.For<IRepository<VehicleEntity>>();
        _vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleTypeEntity>>();
        _buildingRepositoryMock = Substitute.For<IRepository<BuildingEntity>>();
        _buildingDetailRepositoryMock = Substitute.For<IBuildingRepository>();
        _pricingPolicyRepositoryMock = Substitute.For<IPricingPolicyRepository>();
        _sessionRepositoryMock = Substitute.For<IRepository<ParkingSessionEntity>>();
        _parkingSlotRepositoryMock = Substitute.For<IRepository<ParkingSlotEntity>>();
        _paymentRepositoryMock = Substitute.For<IRepository<PaymentEntity>>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _feeCalculationServiceMock = Substitute.For<IFeeCalculationService>();

        _service = new BookingService(
            _bookingRepositoryMock,
            _vehicleRepositoryMock,
            _vehicleTypeRepositoryMock,
            _buildingRepositoryMock,
            _buildingDetailRepositoryMock,
            _pricingPolicyRepositoryMock,
            _sessionRepositoryMock,
            _parkingSlotRepositoryMock,
            _paymentRepositoryMock,
            _unitOfWorkMock
        );
    }

    [Fact]
    public async Task CleanupExpiredBookingsAsync_ShouldTransitionPendingToExpiredAndConfirmedToNoShow()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var pendingBooking = new BookingEntity
        {
            Id = 1,
            BookingStatus = BookingStatus.Pending,
            PaymentDeadline = now.AddMinutes(-5),
            CheckinGraceUntil = now.AddHours(2)
        };

        var confirmedBooking = new BookingEntity
        {
            Id = 2,
            BookingStatus = BookingStatus.Confirmed,
            PaymentDeadline = now.AddMinutes(-30),
            CheckinGraceUntil = now.AddMinutes(-10) // Grace period passed
        };

        // Mock FindAsync for Pending & Confirmed
        _bookingRepositoryMock.FindAsync(Arg.Any<Expression<Func<BookingEntity, bool>>>())
            .Returns(call =>
            {
                var expr = call.Arg<Expression<Func<BookingEntity, bool>>>();
                var compiled = expr.Compile();
                
                var list = new List<BookingEntity> { pendingBooking, confirmedBooking };
                var matched = new List<BookingEntity>();
                foreach (var item in list)
                {
                    if (compiled(item))
                    {
                        matched.Add(item);
                    }
                }
                return matched;
            });

        // Act
        await _service.CleanupExpiredBookingsAsync();

        // Assert
        Assert.Equal(BookingStatus.Expired, pendingBooking.BookingStatus);
        Assert.Equal("Hết hạn thanh toán tiền cọc", pendingBooking.CancelReason);
        _bookingRepositoryMock.Received(1).Update(pendingBooking);

        Assert.Equal(BookingStatus.NoShow, confirmedBooking.BookingStatus);
        Assert.Equal("Khách hàng không đến (No-Show)", confirmedBooking.CancelReason);
        _bookingRepositoryMock.Received(1).Update(confirmedBooking);

        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithValidSlotId_ShouldSucceed()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        var floor = new Floor { BuildingId = 1 };
        var zone = new Zone { FloorId = 1, Floor = floor };
        var slot = new ParkingSlotEntity { Id = 10, VehicleTypeId = 1, Status = SlotStatus.Available, Zone = zone };

        _parkingSlotRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<ParkingSlotEntity, bool>>>())
            .Returns(slot);

        _bookingRepositoryMock.AnyAsync(Arg.Any<Expression<Func<BookingEntity, bool>>>())
            .Returns(false);

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.SlotId);
        await _bookingRepositoryMock.Received(1).AddAsync(Arg.Is<BookingEntity>(b => b.SlotId == 10));
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateBookingAsync_MotorcycleWithSlotId_ShouldThrowDomainException_InvalidSlotSelection()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "29T1-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 2, TypeName = "Motorcycle", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 2, LicensePlate = "29T1-12345", VehicleTypeId = 2, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(2)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 2)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 2, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 2,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 5000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(2, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("INVALID_SLOT_SELECTION", ex.ErrorCode);
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithNonExistentSlotId_ShouldThrowDomainException_SlotNotFound()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 999
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        _parkingSlotRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<ParkingSlotEntity, bool>>>())
            .Returns((ParkingSlotEntity?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("SLOT_NOT_FOUND", ex.ErrorCode);
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithSlotOfDifferentVehicleType_ShouldThrowDomainException_VehicleTypeMismatch()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        var floor = new Floor { BuildingId = 1 };
        var zone = new Zone { FloorId = 1, Floor = floor };
        var slot = new ParkingSlotEntity { Id = 10, VehicleTypeId = 99, Status = SlotStatus.Available, Zone = zone };

        _parkingSlotRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<ParkingSlotEntity, bool>>>())
            .Returns(slot);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("VEHICLE_TYPE_MISMATCH", ex.ErrorCode);
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithBlockedSlot_ShouldThrowDomainException_SlotNotAvailable()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        var floor = new Floor { BuildingId = 1 };
        var zone = new Zone { FloorId = 1, Floor = floor };
        var slot = new ParkingSlotEntity { Id = 10, VehicleTypeId = 1, Status = SlotStatus.Blocked, Zone = zone };

        _parkingSlotRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<ParkingSlotEntity, bool>>>())
            .Returns(slot);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("SLOT_NOT_AVAILABLE", ex.ErrorCode);
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithAlreadyReservedSlot_ShouldThrowDomainException_SlotAlreadyReserved()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        var floor = new Floor { BuildingId = 1 };
        var zone = new Zone { FloorId = 1, Floor = floor };
        var slot = new ParkingSlotEntity { Id = 10, VehicleTypeId = 1, Status = SlotStatus.Available, Zone = zone };

        _parkingSlotRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<ParkingSlotEntity, bool>>>())
            .Returns(slot);

        _bookingRepositoryMock.AnyAsync(Arg.Any<Expression<Func<BookingEntity, bool>>>())
            .Returns(true); // Slot already taken

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("SLOT_ALREADY_RESERVED", ex.ErrorCode);
    }

    [Fact]
    public async Task CreateBookingAsync_CarWithTypeNameNull_ShouldThrowDomainException_InvalidSlotSelection()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            AccountId = 1,
            LicensePlate = "30A-12345",
            BuildingId = 1,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2),
            SlotId = 10
        };

        var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = null!, BufferRatio = 10 };
        var vehicle = new VehicleEntity { Id = 1, LicensePlate = "30A-12345", VehicleTypeId = 1, VehicleType = vehicleType };
        var building = new BuildingEntity { Id = 1, Name = "Building A" };
        
        _vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
            .Returns(new List<VehicleEntity> { vehicle });
            
        _buildingRepositoryMock.GetByIdAsync(1)
            .Returns(building);
            
        _vehicleTypeRepositoryMock.GetByIdAsync(1)
            .Returns(vehicleType);

        _buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1)
            .Returns(100);

        _sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>())
            .Returns(5);

        _bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(5);

        var pricingPolicy = new PricingPolicy
        {
            VehicleTypeId = 1,
            PricingWindows = new List<PricingWindow>
            {
                new PricingWindow { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(24), BasePrice = 20000 }
            }
        };

        _pricingPolicyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
            .Returns(pricingPolicy);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateBookingAsync(request));
        Assert.Equal("INVALID_SLOT_SELECTION", ex.ErrorCode);
    }

    [Fact]
    public async Task CancelBookingAsync_PendingBooking_ShouldCancelWithoutRefund()
    {
        // Arrange
        var booking = new BookingEntity
        {
            Id = 1,
            BookingStatus = BookingStatus.Pending,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2)
        };
        _bookingRepositoryMock.GetByIdAsync(1).Returns(booking);

        // Act
        await _service.CancelBookingAsync(1, "User cancelled");

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.BookingStatus);
        Assert.Equal("User cancelled", booking.CancelReason);
        _bookingRepositoryMock.Received(1).Update(booking);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
        _paymentRepositoryMock.DidNotReceive().Update(Arg.Any<PaymentEntity>());
    }

    [Fact]
    public async Task CancelBookingAsync_ConfirmedBookingMoreThanOneHourBefore_ShouldCancelAndRefund()
    {
        // Arrange
        var booking = new BookingEntity
        {
            Id = 1,
            BookingStatus = BookingStatus.Confirmed,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(2) // 2 hours is > 1 hour
        };
        var payment = new PaymentEntity
        {
            Id = 1,
            BookingId = 1,
            PaymentStatus = "PAID"
        };

        _bookingRepositoryMock.GetByIdAsync(1).Returns(booking);
        _paymentRepositoryMock.FindAsync(Arg.Any<Expression<Func<PaymentEntity, bool>>>())
            .Returns(new List<PaymentEntity> { payment });

        // Act
        await _service.CancelBookingAsync(1, "User cancelled");

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.BookingStatus);
        Assert.Contains("Đã hoàn cọc", booking.CancelReason);
        Assert.Equal("REFUNDED", payment.PaymentStatus);
        _paymentRepositoryMock.Received(1).Update(payment);
        _bookingRepositoryMock.Received(1).Update(booking);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CancelBookingAsync_ConfirmedBookingLessThanOneHourBefore_ShouldCancelAndForfeitDeposit()
    {
        // Arrange
        var booking = new BookingEntity
        {
            Id = 1,
            BookingStatus = BookingStatus.Confirmed,
            PlannedCheckinTime = DateTime.UtcNow.AddMinutes(30) // 30 minutes is < 1 hour
        };
        var payment = new PaymentEntity
        {
            Id = 1,
            BookingId = 1,
            PaymentStatus = "PAID"
        };

        _bookingRepositoryMock.GetByIdAsync(1).Returns(booking);
        _paymentRepositoryMock.FindAsync(Arg.Any<Expression<Func<PaymentEntity, bool>>>())
            .Returns(new List<PaymentEntity> { payment });

        // Act
        await _service.CancelBookingAsync(1, "User cancelled");

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.BookingStatus);
        Assert.Contains("Mất cọc", booking.CancelReason);
        Assert.Equal("PAID", payment.PaymentStatus); // remains paid (forfeited)
        _paymentRepositoryMock.DidNotReceive().Update(payment);
        _bookingRepositoryMock.Received(1).Update(booking);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}


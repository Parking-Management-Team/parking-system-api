using NSubstitute;
using PBMS.Application.Booking.Services;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
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
            _unitOfWorkMock,
            _feeCalculationServiceMock
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
}

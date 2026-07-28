using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSystemConfig.Interfaces;
using PBMS.Application.Pricing.Services;
using PBMS.Domain.Engine;
using PBMS.Domain.Entities;
using Xunit;

namespace PBMS.UnitTests;

public class PricingCalculationLoggingBoundaryTests
{
    private readonly IPricingPolicyRepository _pricingPolicyRepo;
    private readonly IPricingEngine _pricingEngine;
    private readonly IRepository<PricingCalculationLog> _logRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IPenaltyConfigRepository _penaltyConfigRepo;
    private readonly IParkingSystemConfigService _configService;

    private readonly PricingCalculationService _service;

    public PricingCalculationLoggingBoundaryTests()
    {
        _pricingPolicyRepo = Substitute.For<IPricingPolicyRepository>();
        _pricingEngine = Substitute.For<IPricingEngine>();
        _logRepo = Substitute.For<IRepository<PricingCalculationLog>>();
        _incidentRepo = Substitute.For<IIncidentRepository>();
        _penaltyConfigRepo = Substitute.For<IPenaltyConfigRepository>();
        _configService = Substitute.For<IParkingSystemConfigService>();

        _service = new PricingCalculationService(
            _pricingPolicyRepo,
            _pricingEngine,
            _logRepo,
            _incidentRepo,
            _penaltyConfigRepo,
            _configService
        );
    }

    [Fact]
    public async Task CalculatePreviewAsync_DoesNotPersistPricingCalculationLog()
    {
        // Arrange
        var checkIn = DateTime.UtcNow;
        var checkOut = checkIn.AddHours(2);
        var policy = new PricingPolicy { Id = 10 };

        _pricingPolicyRepo
            .GetActivePolicyAsync(1, checkIn)
            .Returns(Task.FromResult<PricingPolicy?>(policy));

        _pricingEngine
            .Calculate(policy, checkIn, checkOut, null, null)
            .Returns(new PricingResult { TotalAmount = 50000 });

        // Act
        var result = await _service.CalculatePreviewAsync(1, checkIn, checkOut);

        // Assert
        Assert.Equal(50000, result.TotalAmount);
        await _logRepo.DidNotReceive().AddAsync(Arg.Any<PricingCalculationLog>());
        await _logRepo.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task CalculateCommittedFeeAsync_PersistsPricingCalculationLogWithPurposeAndIdentifiers()
    {
        // Arrange
        var checkIn = DateTime.UtcNow;
        var checkOut = checkIn.AddHours(3);
        var policy = new PricingPolicy { Id = 12 };
        PricingCalculationLog? savedLog = null;

        _pricingPolicyRepo
            .GetActivePolicyAsync(1, checkIn)
            .Returns(Task.FromResult<PricingPolicy?>(policy));

        _pricingEngine
            .Calculate(policy, checkIn, checkOut, null, null)
            .Returns(new PricingResult { TotalAmount = 75000 });

        _logRepo
            .AddAsync(Arg.Do<PricingCalculationLog>(l => savedLog = l))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CalculateCommittedFeeAsync(
            vehicleTypeId: 1,
            checkIn: checkIn,
            checkOut: checkOut,
            calculationPurpose: "BOOKING_DEPOSIT",
            bookingId: 101,
            paymentId: 501,
            idempotencyKey: "KEY_BOOKING_101"
        );

        // Assert
        Assert.Equal(75000, result.TotalAmount);
        await _logRepo.Received(1).AddAsync(Arg.Any<PricingCalculationLog>());
        await _logRepo.Received(1).SaveChangesAsync();

        Assert.NotNull(savedLog);
        Assert.Equal("BOOKING_DEPOSIT", savedLog!.CalculationPurpose);
        Assert.Equal(101, savedLog.BookingId);
        Assert.Equal(501, savedLog.PaymentId);
        Assert.Equal("KEY_BOOKING_101", savedLog.IdempotencyKey);
        Assert.Equal(75000, savedLog.TotalPrice);
    }

    [Fact]
    public async Task CalculateCommittedFeeAsync_IdempotentReplay_DoesNotDuplicateLog()
    {
        // Arrange
        var checkIn = DateTime.UtcNow;
        var checkOut = checkIn.AddHours(2);
        var existingLog = new PricingCalculationLog
        {
            Id = 999,
            IdempotencyKey = "REPLAY_KEY_123",
            TotalPrice = 60000,
            CalculationPurpose = "SESSION_EXTENSION"
        };

        _logRepo
            .FindAsync(Arg.Any<Expression<Func<PricingCalculationLog, bool>>>())
            .Returns(Task.FromResult<IEnumerable<PricingCalculationLog>>(new List<PricingCalculationLog> { existingLog }));

        // Act
        var result = await _service.CalculateCommittedFeeAsync(
            vehicleTypeId: 1,
            checkIn: checkIn,
            checkOut: checkOut,
            calculationPurpose: "SESSION_EXTENSION",
            idempotencyKey: "REPLAY_KEY_123"
        );

        // Assert
        Assert.Equal(60000, result.TotalAmount);
        await _logRepo.DidNotReceive().AddAsync(Arg.Any<PricingCalculationLog>());
        await _logRepo.DidNotReceive().SaveChangesAsync();
    }
}

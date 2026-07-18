using NSubstitute;
using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Services;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingSystemConfig.Interfaces;
using PBMS.Application.ParkingSystemConfig.DTOs;
using PBMS.Application.Payment.Interfaces;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Application.Pricing.Services;
using PBMS.Domain.Engine;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
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

namespace PBMS.UnitTests
{
    public class PricingPolicySelectionTests
    {
        private readonly IPricingPolicyRepository _policyRepositoryMock;
        private readonly IPricingEngine _pricingEngine;
        private readonly IRepository<PricingCalculationLog> _logRepositoryMock;
        private readonly IIncidentRepository _incidentRepositoryMock;
        private readonly IPenaltyConfigRepository _penaltyConfigRepositoryMock;
        private readonly IParkingSystemConfigService _configServiceMock;
        private readonly PricingCalculationService _pricingCalculationService;

        public PricingPolicySelectionTests()
        {
            _policyRepositoryMock = Substitute.For<IPricingPolicyRepository>();
            _pricingEngine = new PricingEngine(); // Using the actual PricingEngine domain logic
            _logRepositoryMock = Substitute.For<IRepository<PricingCalculationLog>>();
            _incidentRepositoryMock = Substitute.For<IIncidentRepository>();
            _penaltyConfigRepositoryMock = Substitute.For<IPenaltyConfigRepository>();
            _configServiceMock = Substitute.For<IParkingSystemConfigService>();

            _pricingCalculationService = new PricingCalculationService(
                _policyRepositoryMock,
                _pricingEngine,
                _logRepositoryMock,
                _incidentRepositoryMock,
                _penaltyConfigRepositoryMock,
                _configServiceMock
            );
        }

        [Fact]
        public async Task CalculateFeeAsync_ShouldUseSegmentedPricing_WhenSwitchIsTrue()
        {
            // Arrange
            int vehicleTypeId = 1;
            var checkIn = new DateTime(2026, 6, 30, 22, 0, 0); // checked in on June 30th
            var checkOut = new DateTime(2026, 7, 1, 2, 0, 0);   // checked out on July 1st (total 4 hours)

            // Segmented switch is set to "true"
            _configServiceMock.GetByKeyAsync("APPLY_SEGMENTED_PRICING")
                .Returns(Task.FromResult<ParkingSystemConfigDto?>(new ParkingSystemConfigDto { Value = "true" }));

            // 1. Expired Policy ending June 30th
            var expiredPolicy = new PricingPolicy
            {
                Id = 101,
                VehicleTypeId = vehicleTypeId,
                PolicyName = "Old Spring Policy",
                EffectiveStart = new DateTime(2026, 1, 1),
                EffectiveEnd = new DateTime(2026, 6, 30),
                PricingPolicyStatus = "Expired"
            };
            expiredPolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 10000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 5000m }
                }
            };

            // 2. Active Policy starting July 1st
            var activePolicy = new PricingPolicy
            {
                Id = 102,
                VehicleTypeId = vehicleTypeId,
                PolicyName = "New Summer Policy",
                EffectiveStart = new DateTime(2026, 7, 1),
                EffectiveEnd = null,
                PricingPolicyStatus = "Active"
            };
            activePolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 20000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 10000m }
                }
            };

            // Retrieve policies intersecting with the [June 30th - July 1st] range
            _policyRepositoryMock.GetPoliciesInPeriodAsync(vehicleTypeId, checkIn, checkOut)
                .Returns(Task.FromResult(new List<PricingPolicy> { expiredPolicy, activePolicy }));

            // Act
            var pricingResult = await _pricingCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut);

            // Assert
            // 4 hours: 
            // - Segment 1 (June 30, 22:00 -> July 1, 00:00 = 2 hours): 
            //   - Hour 1 (BasePrice = 10,000) + Hour 2 (Increment = 5,000) = 15,000 VND
            // - Segment 2 (July 1, 00:00 -> July 1, 02:00 = 2 hours):
            //   - Hour 3 (Increment = 10,000) + Hour 4 (Increment = 10,000) = 20,000 VND
            //   Note: Base block is ONLY charged once at the beginning, so Segment 2 applies increment price (10,000 VND/hour).
            // Total = 15,000 + 20,000 = 35,000 VND.
            Assert.Equal(35000m, pricingResult.TotalAmount);
            await _policyRepositoryMock.Received(1).GetPoliciesInPeriodAsync(vehicleTypeId, checkIn, checkOut);
        }

        [Fact]
        public async Task CalculateFeeAsync_ShouldUseHighestPriorityPolicy_WhenOverlappingPoliciesExist()
        {
            // Arrange
            int vehicleTypeId = 1;
            var checkIn = new DateTime(2026, 7, 18, 10, 0, 0); // July 18th
            var checkOut = checkIn.AddHours(4); // 4 hours stay

            _configServiceMock.GetByKeyAsync("APPLY_SEGMENTED_PRICING")
                .Returns(Task.FromResult<ParkingSystemConfigDto?>(new ParkingSystemConfigDto { Value = "true" }));

            // 1. Default policy (Priority = 0)
            var defaultPolicy = new PricingPolicy
            {
                Id = 201,
                VehicleTypeId = vehicleTypeId,
                PolicyName = "Default Casual Policy",
                EffectiveStart = new DateTime(2026, 1, 1),
                EffectiveEnd = null,
                PricingPolicyStatus = "Active",
                Priority = 0
            };
            defaultPolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 20000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 10000m }
                }
            };

            // 2. Special Holiday policy (Priority = 1, covers July 18th)
            var holidayPolicy = new PricingPolicy
            {
                Id = 202,
                VehicleTypeId = vehicleTypeId,
                PolicyName = "Holiday Special Policy",
                EffectiveStart = new DateTime(2026, 7, 18),
                EffectiveEnd = new DateTime(2026, 7, 18),
                PricingPolicyStatus = "Active",
                Priority = 1 // Higher priority!
            };
            holidayPolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 40000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 20000m }
                }
            };

            _policyRepositoryMock.GetPoliciesInPeriodAsync(vehicleTypeId, checkIn, checkOut)
                .Returns(Task.FromResult(new List<PricingPolicy> { defaultPolicy, holidayPolicy }));

            // Act
            var pricingResult = await _pricingCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut);

            // Assert
            // 4 hours under holiday pricing: 40k + 3 * 20k = 100,000 VND
            Assert.Equal(100000m, pricingResult.TotalAmount);
        }

        [Fact]
        public async Task CalculateFeeAsync_ShouldUseCheckInPolicy_WhenSwitchIsFalse()
        {
            // Arrange
            int vehicleTypeId = 1;
            var checkIn = new DateTime(2026, 6, 30, 22, 0, 0); 
            var checkOut = new DateTime(2026, 7, 1, 2, 0, 0); 

            // Switch is off (false)
            _configServiceMock.GetByKeyAsync("APPLY_SEGMENTED_PRICING")
                .Returns(Task.FromResult<ParkingSystemConfigDto?>(new ParkingSystemConfigDto { Value = "false" }));

            var expiredPolicy = new PricingPolicy
            {
                Id = 101,
                VehicleTypeId = vehicleTypeId,
                PolicyName = "Old Spring Policy",
                EffectiveStart = new DateTime(2026, 1, 1),
                EffectiveEnd = new DateTime(2026, 6, 30),
                PricingPolicyStatus = "Expired"
            };
            expiredPolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 10000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 5000m }
                }
            };

            // Repo returns the check-in policy (using expired fallback)
            _policyRepositoryMock.GetActivePolicyAsync(vehicleTypeId, checkIn)
                .Returns(Task.FromResult<PricingPolicy?>(expiredPolicy));

            // Act
            var pricingResult = await _pricingCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut);

            // Assert
            // 4 hours solely calculated under expiredPolicy: 10k (base) + 3 * 5k (increment) = 25,000 VND
            Assert.Equal(25000m, pricingResult.TotalAmount);
            await _policyRepositoryMock.Received(1).GetActivePolicyAsync(vehicleTypeId, checkIn);
        }

        [Fact]
        public async Task BookingService_ShouldCalculateDepositWithActivePolicy_WhenCreatingBooking()
        {
            // Arrange
            var bookingRepositoryMock = Substitute.For<IBookingRepository>();
            var vehicleRepositoryMock = Substitute.For<IRepository<VehicleEntity>>();
            var vehicleTypeRepositoryMock = Substitute.For<IRepository<VehicleTypeEntity>>();
            var buildingRepositoryMock = Substitute.For<IRepository<BuildingEntity>>();
            var buildingDetailRepositoryMock = Substitute.For<IBuildingRepository>();
            var sessionRepositoryMock = Substitute.For<IRepository<ParkingSessionEntity>>();
            var parkingSlotRepositoryMock = Substitute.For<IParkingSlotRepository>();
            var paymentRepositoryMock = Substitute.For<IRepository<PaymentEntity>>();
            var unitOfWorkMock = Substitute.For<IUnitOfWork>();
            var configurationMock = Substitute.For<IConfiguration>();
            var blacklistRepositoryMock = Substitute.For<IBlacklistRepository>();
            var vnpayGatewayMock = Substitute.For<IVNPayGateway>();
            var zoneCapacityRepositoryMock = Substitute.For<IZoneBookingCapacityRepository>();

            var plannedCheckin = DateTime.UtcNow.AddHours(2);
            var plannedCheckout = plannedCheckin.AddHours(4);

            var request = new CreateBookingRequest
            {
                AccountId = 1,
                LicensePlate = "30A-99999",
                BuildingId = 1,
                PlannedCheckinTime = plannedCheckin,
                PlannedCheckoutTime = plannedCheckout
            };

            var vehicleType = new VehicleTypeEntity { Id = 1, TypeName = "Car", BufferRatio = 10 };
            var vehicle = new VehicleEntity { Id = 5, LicensePlate = "30A-99999", VehicleTypeId = 1, VehicleType = vehicleType };
            var building = new BuildingEntity { Id = 1, Name = "Building A" };

            // Mock config switch to off
            _configServiceMock.GetByKeyAsync("APPLY_SEGMENTED_PRICING")
                .Returns(Task.FromResult<ParkingSystemConfigDto?>(null));

            vehicleRepositoryMock.FindAsync(Arg.Any<Expression<Func<VehicleEntity, bool>>>())
                .Returns(new List<VehicleEntity> { vehicle });
            buildingRepositoryMock.GetByIdAsync(1).Returns(building);
            vehicleTypeRepositoryMock.GetByIdAsync(1).Returns(vehicleType);
            buildingDetailRepositoryMock.GetTotalGeneralCapacityAsync(1, 1).Returns(100);
            sessionRepositoryMock.CountAsync(Arg.Any<Expression<Func<ParkingSessionEntity, bool>>>()).Returns(0);
            bookingRepositoryMock.GetActiveBookingsCountAsync(1, 1, Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(0);

            // Active Policy Setup
            var activePolicy = new PricingPolicy
            {
                Id = 102,
                VehicleTypeId = 1,
                PolicyName = "Active Summer Policy",
                EffectiveStart = DateTime.UtcNow.Date.AddDays(-10),
                EffectiveEnd = null,
                PricingPolicyStatus = "Active"
            };

            activePolicy.PricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleType = "BasePricing", IsActive = true, ExecutionOrder = 1,
                    BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 30000m }
                },
                new PricingRule
                {
                    RuleType = "IncrementPricing", IsActive = true, ExecutionOrder = 2,
                    IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 15000m }
                }
            };

            _policyRepositoryMock.GetActivePolicyAsync(1, Arg.Any<DateTime>())
                .Returns(Task.FromResult<PricingPolicy?>(activePolicy));

            var bookingService = new BookingService(
                bookingRepositoryMock,
                vehicleRepositoryMock,
                vehicleTypeRepositoryMock,
                buildingRepositoryMock,
                buildingDetailRepositoryMock,
                _policyRepositoryMock,
                sessionRepositoryMock,
                parkingSlotRepositoryMock,
                paymentRepositoryMock,
                unitOfWorkMock,
                configurationMock,
                blacklistRepositoryMock,
                vnpayGatewayMock,
                _pricingCalculationService, // Pass real pricing service
                _configServiceMock, // Config service mock
                zoneCapacityRepositoryMock
            );

            // Act
            var result = await bookingService.CreateBookingAsync(request);

            // Assert
            // 4 hours: 30k + 3 * 15k = 75,000 VND
            Assert.Equal(75000m, result.DepositAmount);
            Assert.Equal(BookingStatus.Pending, result.BookingStatus);
            await unitOfWorkMock.Received(1).SaveChangesAsync();
        }
    }
}

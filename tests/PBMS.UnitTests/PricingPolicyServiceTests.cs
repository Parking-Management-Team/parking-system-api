using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.UnitTests;

/// <summary>
/// Unit Tests cho PricingPolicyService — kiểm tra các Business Rules quan trọng:
///
///   [Scenario 1 Create]  : Tạo policy INACTIVE, validate params từng window
///   [Scenario 1 Activate]: Kích hoạt policy — validate 24h coverage + no-overlap
///   [BR-FEE-025]         : Không overlap với policy khác cùng VehicleType
///   [BR-FEE-027/028]     : Windows phủ đủ 24h và không overlap
///   [BR-FEE-029]         : Không cho sửa config giá khi Policy ACTIVE
///   [BR-FEE-031]         : Không cho sửa effective_start khi Policy ACTIVE
/// </summary>
public class PricingPolicyServiceTests
{
    private readonly IPricingPolicyRepository _policyRepoMock;
    private readonly IRepository<VehicleType> _vehicleTypeRepoMock;
    private readonly PricingPolicyService _service;

    public PricingPolicyServiceTests()
    {
        _policyRepoMock = Substitute.For<IPricingPolicyRepository>();
        _vehicleTypeRepoMock = Substitute.For<IRepository<VehicleType>>();
        _service = new PricingPolicyService(_policyRepoMock, _vehicleTypeRepoMock);
    }

    // =========================================================================
    // Helper: tạo VehicleType mock
    // =========================================================================
    private static VehicleType MakeVehicleType(int id = 1) =>
        new VehicleType { Id = id, TypeName = VehicleType.MotorcycleTypeName };

    // Helper: tạo PricingWindow cho test
    private static PricingWindow MakeWindow(
        int id,
        string name,
        TimeSpan start,
        TimeSpan end) =>
        new PricingWindow
        {
            Id = id,
            WindowName = name,
            StartTime = start,
            EndTime = end,
            BaseDurationMinutes = 60,
            BasePrice = 5000m,
            IncrementBlockMinutes = 15,
            IncrementPrice = 2000m,
            WindowCap = null,
            GracePeriodMinutes = 0
        };

    // Helper: tạo CreatePricingWindowRequest hợp lệ phủ 24h (1 window toàn ngày)
    private static CreatePricingWindowRequest MakeFullDayWindowRequest() =>
        new CreatePricingWindowRequest
        {
            WindowName = "Full Day",
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.Zero, // StartTime == EndTime → 24h
            BaseDurationMinutes = 60,
            BasePrice = 5000m,
            IncrementBlockMinutes = 15,
            IncrementPrice = 2000m
        };

    // Helper: tạo Policy INACTIVE với windows
    private static PricingPolicy MakeInactivePolicy(int id = 1, int vehicleTypeId = 1) =>
        new PricingPolicy
        {
            Id = id,
            VehicleTypeId = vehicleTypeId,
            PolicyName = "Test Policy",
            EffectiveStart = DateTime.Today,
            EffectiveEnd = null,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = vehicleTypeId, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                // Window phủ đủ 24h (StartTime == EndTime)
                MakeWindow(1, "Full Day", TimeSpan.Zero, TimeSpan.Zero)
            }
        };

    // =========================================================================
    // [Scenario 1 - Create] Tạo Policy INACTIVE
    // =========================================================================

    /// <summary>
    /// Tạo policy thành công → status phải là "Inactive" (không phải "Active").
    /// </summary>
    [Fact]
    public async Task CreatePricingPolicyAsync_ShouldCreateWithInactiveStatus()
    {
        // Arrange
        var vehicleType = MakeVehicleType();
        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(Task.FromResult<VehicleType?>(vehicleType));
        _policyRepoMock.AddAsync(Arg.Any<PricingPolicy>()).Returns(Task.CompletedTask);
        _policyRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));

        var request = new CreatePricingPolicyRequest
        {
            VehicleTypeId = 1,
            PolicyName = "Motorcycle Pricing",
            EffectiveStart = DateTime.Today,
            PricingWindows = new List<CreatePricingWindowRequest> { MakeFullDayWindowRequest() }
        };

        // Act
        var result = await _service.CreatePricingPolicyAsync(request);

        // Assert: policy tạo ra phải ở trạng thái Inactive
        Assert.Equal("Inactive", result.PricingPolicyStatus);
        Assert.Equal("Motorcycle Pricing", result.PolicyName);
        Assert.Equal(1, result.VehicleTypeId);
    }

    /// <summary>
    /// Tạo policy với VehicleTypeId không tồn tại → DomainException "VEHICLE_TYPE_NOT_FOUND".
    /// </summary>
    [Fact]
    public async Task CreatePricingPolicyAsync_ShouldThrow_WhenVehicleTypeNotFound()
    {
        _vehicleTypeRepoMock.GetByIdAsync(999).Returns(Task.FromResult<VehicleType?>(null));

        var request = new CreatePricingPolicyRequest
        {
            VehicleTypeId = 999,
            PolicyName = "Test",
            EffectiveStart = DateTime.Today,
            PricingWindows = new List<CreatePricingWindowRequest> { MakeFullDayWindowRequest() }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.CreatePricingPolicyAsync(request));
        Assert.Equal("VEHICLE_TYPE_NOT_FOUND", ex.ErrorCode);
    }

    /// <summary>
    /// Tạo policy với EffectiveEnd <= EffectiveStart → DomainException "INVALID_EFFECTIVE_DATE_RANGE".
    /// </summary>
    [Fact]
    public async Task CreatePricingPolicyAsync_ShouldThrow_WhenEffectiveEndBeforeStart()
    {
        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(Task.FromResult<VehicleType?>(MakeVehicleType()));

        var request = new CreatePricingPolicyRequest
        {
            VehicleTypeId = 1,
            PolicyName = "Test",
            EffectiveStart = DateTime.Today,
            EffectiveEnd = DateTime.Today.AddDays(-1), // End < Start
            PricingWindows = new List<CreatePricingWindowRequest> { MakeFullDayWindowRequest() }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.CreatePricingPolicyAsync(request));
        Assert.Equal("INVALID_EFFECTIVE_DATE_RANGE", ex.ErrorCode);
    }

    /// <summary>
    /// Tạo policy với WindowCap < BasePrice → DomainException "WINDOW_CAP_BELOW_BASE_PRICE".
    /// </summary>
    [Fact]
    public async Task CreatePricingPolicyAsync_ShouldThrow_WhenWindowCapBelowBasePrice()
    {
        _vehicleTypeRepoMock.GetByIdAsync(1).Returns(Task.FromResult<VehicleType?>(MakeVehicleType()));

        var request = new CreatePricingPolicyRequest
        {
            VehicleTypeId = 1,
            PolicyName = "Test",
            EffectiveStart = DateTime.Today,
            PricingWindows = new List<CreatePricingWindowRequest>
            {
                new CreatePricingWindowRequest
                {
                    WindowName = "Invalid Window",
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.Zero,
                    BaseDurationMinutes = 60,
                    BasePrice = 10000m,
                    IncrementBlockMinutes = 15,
                    IncrementPrice = 2000m,
                    WindowCap = 5000m // WindowCap < BasePrice
                }
            }
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.CreatePricingPolicyAsync(request));
        Assert.Equal("WINDOW_CAP_BELOW_BASE_PRICE", ex.ErrorCode);
    }

    // =========================================================================
    // [Scenario 1 - Activate] Kích hoạt Policy
    // =========================================================================

    /// <summary>
    /// Kích hoạt policy thành công khi windows phủ đủ 24h và không overlap.
    /// Status phải chuyển sang "Active".
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldSucceed_WhenWindowsCover24Hours()
    {
        // Arrange: policy INACTIVE với 2 windows phủ đủ 24h
        var policy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Test Policy",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                MakeWindow(1, "Day Time Window", new TimeSpan(6, 0, 0), new TimeSpan(22, 0, 0)),
                MakeWindow(2, "Night Time Window", new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0))
            }
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);
        _policyRepoMock.HasOverlapPolicyAsync(1, policy.EffectiveStart, null, 1).Returns(false);
        _policyRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));

        // Act
        var result = await _service.ActivatePricingPolicyAsync(1);

        // Assert
        Assert.Equal("Active", result.PricingPolicyStatus);
    }

    /// <summary>
    /// Kích hoạt policy đã ACTIVE → DomainException "POLICY_ALREADY_ACTIVE_OR_EXPIRED".
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldThrow_WhenPolicyAlreadyActive()
    {
        var policy = MakeInactivePolicy();
        policy.PricingPolicyStatus = "Active"; // Đã active rồi

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ActivatePricingPolicyAsync(1));
        Assert.Equal("POLICY_ALREADY_ACTIVE_OR_EXPIRED", ex.ErrorCode);
    }

    /// <summary>
    /// [BR-FEE-027/028] Kích hoạt policy với windows không phủ đủ 24h (có gap) →
    /// DomainException "WINDOWS_DO_NOT_COVER_24_HOURS".
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldThrow_WhenWindowsDoNotCover24Hours()
    {
        // Arrange: chỉ có 1 window 06:00 - 22:00 (không phủ 22:00 - 06:00)
        var policy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Missing Window",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                MakeWindow(1, "Day Time Window", new TimeSpan(6, 0, 0), new TimeSpan(22, 0, 0))
                // Thiếu window đêm: 22:00 - 06:00
            }
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ActivatePricingPolicyAsync(1));
        Assert.Equal("WINDOWS_DO_NOT_COVER_24_HOURS", ex.ErrorCode);
    }

    /// <summary>
    /// [BR-FEE-027] Kích hoạt policy với windows overlap nhau →
    /// DomainException "WINDOWS_OVERLAP".
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldThrow_WhenWindowsOverlap()
    {
        // Arrange: 2 windows bị chồng nhau (06:00-22:00 và 20:00-06:00 → overlap 20:00-22:00)
        var policy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Windows overlap",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                MakeWindow(1, "Window A", new TimeSpan(6, 0, 0), new TimeSpan(22, 0, 0)),
                MakeWindow(2, "Window B", new TimeSpan(20, 0, 0), new TimeSpan(6, 0, 0)) // Overlap 20-22h
            }
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ActivatePricingPolicyAsync(1));
        Assert.Equal("WINDOWS_OVERLAP", ex.ErrorCode);
    }

    /// <summary>
    /// [BR-FEE-025] Kích hoạt policy khi đã tồn tại policy khác cùng VehicleType overlap →
    /// DomainException "POLICY_OVERLAP".
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldThrow_WhenPolicyOverlapExists()
    {
        // Arrange: policy INACTIVE với 2 windows phủ đủ 24h (valid windows)
        var policy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Test Policy",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                MakeWindow(1, "Day Time Window", new TimeSpan(6, 0, 0), new TimeSpan(22, 0, 0)),
                MakeWindow(2, "Night Time Window", new TimeSpan(22, 0, 0), new TimeSpan(6, 0, 0))
            }
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);
        // Simulate: tồn tại policy khác overlap
        _policyRepoMock.HasOverlapPolicyAsync(1, policy.EffectiveStart, null, 1).Returns(true);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.ActivatePricingPolicyAsync(1));
        Assert.Equal("POLICY_OVERLAP", ex.ErrorCode);
    }

    // =========================================================================
    // [BR-FEE-029] Không cho sửa config giá khi Policy ACTIVE
    // =========================================================================

    /// <summary>
    /// [BR-FEE-029] AddPricingWindow vào Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY".
    /// </summary>
    [Fact]
    public async Task AddPricingWindowAsync_ShouldThrow_WhenPolicyIsActive()
    {
        var activePolicy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Active Policy",
            PricingPolicyStatus = "Active"
        };

        _policyRepoMock.GetByIdAsync(1).Returns(activePolicy);

        var request = new CreatePricingWindowRequest
        {
            WindowName = "New Window",
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.Zero,
            BaseDurationMinutes = 60,
            BasePrice = 5000m,
            IncrementBlockMinutes = 15,
            IncrementPrice = 2000m
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.AddPricingWindowAsync(1, request));
        Assert.Equal("CANNOT_MODIFY_ACTIVE_POLICY", ex.ErrorCode);
    }

    /// <summary>
    /// [BR-FEE-029] UpdatePricingWindow của Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY".
    /// </summary>
    [Fact]
    public async Task UpdatePricingWindowAsync_ShouldThrow_WhenPolicyIsActive()
    {
        var activePolicy = new PricingPolicy
        {
            Id = 1,
            PricingPolicyStatus = "Active"
        };

        var window = new PricingWindow
        {
            Id = 10,
            PricingPolicyId = 1,
            WindowName = "Window A",
            BaseDurationMinutes = 60,
            BasePrice = 5000m,
            IncrementBlockMinutes = 15,
            IncrementPrice = 2000m,
            PricingPolicy = activePolicy
        };

        _policyRepoMock.GetWindowByIdAsync(10).Returns(window);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdatePricingWindowAsync(10, new UpdatePricingWindowRequest
            {
                BasePrice = 9999m // Cố tình thay đổi giá
            }));
        Assert.Equal("CANNOT_MODIFY_ACTIVE_POLICY", ex.ErrorCode);
    }

    /// <summary>
    /// [BR-FEE-029] DeletePricingWindow của Policy đã ACTIVE → DomainException "CANNOT_MODIFY_ACTIVE_POLICY".
    /// </summary>
    [Fact]
    public async Task DeletePricingWindowAsync_ShouldThrow_WhenPolicyIsActive()
    {
        var activePolicy = new PricingPolicy
        {
            Id = 1,
            PricingPolicyStatus = "Active"
        };

        var window = new PricingWindow
        {
            Id = 10,
            PricingPolicyId = 1,
            PricingPolicy = activePolicy
        };

        _policyRepoMock.GetWindowByIdAsync(10).Returns(window);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.DeletePricingWindowAsync(10));
        Assert.Equal("CANNOT_MODIFY_ACTIVE_POLICY", ex.ErrorCode);
    }

    // =========================================================================
    // [BR-FEE-031] Không cho sửa effective_start khi Policy ACTIVE
    // =========================================================================

    /// <summary>
    /// [BR-FEE-031] UpdatePricingPolicyAsync cố sửa EffectiveStart khi ACTIVE →
    /// DomainException "CANNOT_MODIFY_ACTIVE_POLICY_START_DATE".
    /// </summary>
    [Fact]
    public async Task UpdatePricingPolicyAsync_ShouldThrow_WhenModifyingStartDateOfActivePolicy()
    {
        var activePolicy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Active Policy",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Active",
            PricingWindows = new List<PricingWindow>()
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(activePolicy);

        var request = new UpdatePricingPolicyRequest
        {
            EffectiveStart = DateTime.Today.AddDays(5) // Cố sửa effective_start
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdatePricingPolicyAsync(1, request));
        Assert.Equal("CANNOT_MODIFY_ACTIVE_POLICY_START_DATE", ex.ErrorCode);
    }

    /// <summary>
    /// Update Policy ACTIVE với chỉ PolicyName → thành công (không bị khóa).
    /// </summary>
    [Fact]
    public async Task UpdatePricingPolicyAsync_ShouldSucceed_WhenOnlyUpdatingPolicyName()
    {
        var activePolicy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Old Name",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Active",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>()
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(activePolicy);
        _policyRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));

        var request = new UpdatePricingPolicyRequest
        {
            PolicyName = "New Name" // Only rename - allowed when ACTIVE
        };

        // Act (không ném exception)
        var result = await _service.UpdatePricingPolicyAsync(1, request);

        // Assert
        Assert.Equal("New Name", result.PolicyName);
        Assert.Equal("Active", result.PricingPolicyStatus);
    }

    // =========================================================================
    // [BR-FEE-032] Policy EXPIRED không được sửa EffectiveEnd
    // =========================================================================

    /// <summary>
    /// [BR-FEE-032] UpdatePricingPolicyAsync cố sửa EffectiveEnd của Policy EXPIRED →
    /// DomainException "CANNOT_MODIFY_EXPIRED_POLICY".
    /// </summary>
    [Fact]
    public async Task UpdatePricingPolicyAsync_ShouldThrow_WhenModifyingEffectiveEndOfExpiredPolicy()
    {
        var expiredPolicy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "Expired Policy",
            EffectiveStart = DateTime.Today.AddDays(-30),
            EffectiveEnd = DateTime.Today.AddDays(-1),
            PricingPolicyStatus = "Expired",
            PricingWindows = new List<PricingWindow>()
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(expiredPolicy);

        var request = new UpdatePricingPolicyRequest
        {
            EffectiveEnd = DateTime.Today.AddDays(30) // Cố sửa effective_end của Expired Policy
        };

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _service.UpdatePricingPolicyAsync(1, request));
        Assert.Equal("CANNOT_MODIFY_EXPIRED_POLICY", ex.ErrorCode);
    }

    // =========================================================================
    // ValidateWindowsCover24Hours — Edge Cases
    // =========================================================================

    /// <summary>
    /// Kích hoạt policy với 1 window duy nhất chiếm đúng 24h (StartTime == EndTime) →
    /// thành công.
    /// </summary>
    [Fact]
    public async Task ActivatePricingPolicyAsync_ShouldSucceed_WithSingleFullDayWindow()
    {
        // Arrange: 1 window StartTime = EndTime = 00:00 → phủ đủ 24h
        var policy = new PricingPolicy
        {
            Id = 1,
            VehicleTypeId = 1,
            PolicyName = "24h Policy",
            EffectiveStart = DateTime.Today,
            PricingPolicyStatus = "Inactive",
            VehicleType = new VehicleType { Id = 1, TypeName = VehicleType.MotorcycleTypeName },
            PricingWindows = new List<PricingWindow>
            {
                MakeWindow(1, "Full Day", TimeSpan.Zero, TimeSpan.Zero)
            }
        };

        _policyRepoMock.GetByIdWithWindowsAsync(1).Returns(policy);
        _policyRepoMock.HasOverlapPolicyAsync(1, policy.EffectiveStart, null, 1).Returns(false);
        _policyRepoMock.SaveChangesAsync().Returns(Task.FromResult(1));

        // Act
        var result = await _service.ActivatePricingPolicyAsync(1);

        // Assert
        Assert.Equal("Active", result.PricingPolicyStatus);
    }
}

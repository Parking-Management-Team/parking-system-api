using NSubstitute;
using PBMS.Application.Contracts;
using PBMS.Application.Pricing.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.UnitTests;

/// <summary>
/// Unit Tests cho FeeCalculationService — kiểm tra 4 Acceptance Criteria PBMS-86:
///
///   Scenario 1: Kích hoạt chính sách giá mới thành công
///               (kiểm tra kết quả tính phí sau khi apply bảng giá)
///   Scenario 2: Tính phí chính xác khi xe đỗ qua khung giờ
///               (xe máy đỗ vắt qua nhiều PricingWindow)
///   Scenario 3: Áp dụng giới hạn mức phí trần theo khung giờ (WindowCap)
///   Scenario 4: Xử lý thời gian ân hạn không tính tiền (GracePeriod)
/// </summary>
public class FeeCalculationServiceTests
{
    private readonly IPricingPolicyRepository _policyRepositoryMock;
    private readonly FeeCalculationService _feeCalculationService;

    public FeeCalculationServiceTests()
    {
        _policyRepositoryMock = Substitute.For<IPricingPolicyRepository>();
        _feeCalculationService = new FeeCalculationService(_policyRepositoryMock);
    }

    // =========================================================================
    // Scenario 2: Tính phí chính xác khi xe đỗ qua khung giờ
    // =========================================================================

    /// <summary>
    /// Xe máy gửi trong 1 khung giờ, thời gian nằm trong BaseDuration.
    /// → Chỉ tính BasePrice, không có block lũy tiến.
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldApplyBasePrice_WhenParkingWithinBaseDuration()
    {
        // Arrange
        // Window: 06:00 - 22:00
        // BaseDuration = 60 phút, BasePrice = 5000đ
        // IncrementBlock = 15 phút, IncrementPrice = 2000đ
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 2000m,
                WindowCap = null,
                GracePeriodMinutes = 0
            }
        };

        // Xe vào 08:00, ra 08:45 = 45 phút ≤ 60 phút (BaseDuration)
        var checkIn = new DateTime(2024, 1, 15, 8, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 8, 45, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert
        Assert.Equal(5000m, result.TotalFee);
        Assert.Single(result.Details);

        var detail = result.Details[0];
        Assert.Equal(5000m, detail.BaseCharge);
        Assert.Equal(0, detail.IncrementBlocks);
        Assert.Equal(0m, detail.IncrementCharge);
        Assert.Equal(5000m, detail.RawFee);
        Assert.Equal(5000m, detail.CappedFee);
    }

    /// <summary>
    /// Scenario 2: Xe đỗ qua 2 khung giờ — tách thời gian chính xác theo từng window.
    ///
    /// Setup:
    ///   Window ngày : 06:00 - 22:00, Base 60 phút / 5000đ, Increment 15 phút / 2000đ
    ///   Window đêm  : 22:00 - 06:00, Base 60 phút / 10000đ, Increment 30 phút / 5000đ
    ///
    /// Scenario:
    ///   Xe vào   : 21:00 (đêm 14/1)
    ///   Xe ra    : 23:30 (đêm 14/1)
    ///   Tổng thời gian: 150 phút, vắt qua ranh giới 22:00
    ///
    /// Tính toán dự kiến:
    ///   Đoạn 1 (Window ngày 21:00–22:00 = 60 phút):
    ///     = BaseDuration (60p) → BaseCharge = 5000đ, Increment = 0
    ///     → CappedFee = 5000đ
    ///
    ///   Đoạn 2 (Window đêm 22:00–23:30 = 90 phút):
    ///     BaseDuration = 60p → BaseCharge = 10000đ
    ///     OverMinutes = 90 - 60 = 30 phút > GracePeriod (0)
    ///     IncrementBlocks = Ceiling(30/30) = 1 block → IncrementCharge = 5000đ
    ///     → RawFee = 15000đ, WindowCap = null → CappedFee = 15000đ
    ///
    ///   Tổng = 5000 + 15000 = 20000đ
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldSplitByWindowBoundary_WhenParkingSpansTwoWindows()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 2000m,
                WindowCap = null,
                GracePeriodMinutes = 0
            },
            new PricingWindow
            {
                Id = 2,
                WindowName = "Night Time Window",
                StartTime = new TimeSpan(22, 0, 0),
                EndTime = new TimeSpan(6, 0, 0), // qua đêm
                BaseDurationMinutes = 60,
                BasePrice = 10000m,
                IncrementBlockMinutes = 30,
                IncrementPrice = 5000m,
                WindowCap = null,
                GracePeriodMinutes = 0
            }
        };

        var checkIn = new DateTime(2024, 1, 14, 21, 0, 0);
        var checkOut = new DateTime(2024, 1, 14, 23, 30, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert
        Assert.Equal(20000m, result.TotalFee);
        Assert.Equal(2, result.Details.Count);

        // Đoạn 1: Window ngày (21:00–22:00)
        var detail1 = result.Details.First(d => d.PricingWindowId == 1);
        Assert.Equal(5000m, detail1.BaseCharge);
        Assert.Equal(0, detail1.IncrementBlocks);
        Assert.Equal(5000m, detail1.CappedFee);

        // Đoạn 2: Window đêm (22:00–23:30)
        var detail2 = result.Details.First(d => d.PricingWindowId == 2);
        Assert.Equal(10000m, detail2.BaseCharge);
        Assert.Equal(1, detail2.IncrementBlocks);
        Assert.Equal(5000m, detail2.IncrementCharge);
        Assert.Equal(15000m, detail2.CappedFee);
    }

    // =========================================================================
    // Scenario 3: Áp dụng giới hạn mức phí trần (WindowCap)
    // =========================================================================

    /// <summary>
    /// Scenario 3: WindowCap = 50000đ — khi phí lũy tiến vượt quá WindowCap,
    /// hệ thống tự động giới hạn dừng ở mức 50000đ.
    ///
    /// Setup:
    ///   Window ban ngày: BaseDuration=60p / 5000đ, Increment=15p / 5000đ, WindowCap=50000đ
    ///
    /// Xe đỗ 200 phút:
    ///   BaseCharge = 5000đ
    ///   OverMinutes = 200 - 60 = 140 phút
    ///   IncrementBlocks = Ceiling(140/15) = 10 blocks
    ///   IncrementCharge = 10 × 5000 = 50000đ
    ///   RawFee = 5000 + 50000 = 55000đ
    ///   → CappedFee = Min(55000, 50000) = 50000đ ← WindowCap áp dụng
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldCapFeeAtWindowCap_WhenFeeExceedsCap()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 5000m,
                WindowCap = 50000m,  // Giới hạn trần 50000đ (Scenario 3)
                GracePeriodMinutes = 0
            }
        };

        // Xe vào 08:00, ra 11:20 → 200 phút
        var checkIn = new DateTime(2024, 1, 15, 8, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 11, 20, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert
        Assert.Single(result.Details);
        var detail = result.Details[0];

        Assert.Equal(5000m, detail.BaseCharge);
        Assert.Equal(10, detail.IncrementBlocks);
        Assert.Equal(50000m, detail.IncrementCharge);
        Assert.Equal(55000m, detail.RawFee);         // Phí gốc trước khi cap
        Assert.Equal(50000m, detail.CappedFee);       // Sau khi áp WindowCap
        Assert.Equal(50000m, result.TotalFee);         // Tổng bằng CappedFee
    }

    /// <summary>
    /// Bổ sung: Khi phí chưa đến WindowCap thì không áp dụng cap.
    /// RawFee &lt; WindowCap → CappedFee = RawFee.
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldNotCapFee_WhenFeeIsUnderWindowCap()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 2000m,
                WindowCap = 50000m,
                GracePeriodMinutes = 0
            }
        };

        // Xe đỗ 75 phút: Base 5000 + 1 block × 2000 = 7000đ < 50000đ
        var checkIn = new DateTime(2024, 1, 15, 8, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 9, 15, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert
        Assert.Equal(7000m, result.TotalFee);
        var detail = result.Details[0];
        Assert.Equal(7000m, detail.RawFee);
        Assert.Equal(7000m, detail.CappedFee); // Không bị cap
    }

    // =========================================================================
    // Scenario 4: Xử lý thời gian ân hạn không tính tiền (GracePeriod)
    // =========================================================================

    /// <summary>
    /// Scenario 4: GracePeriod = 10 phút, xe đỗ quá block 8 phút.
    /// → 8 phút &lt;= 10 phút (GracePeriod) → KHÔNG tính block mới.
    ///
    /// Setup:
    ///   Window: BaseDuration=60p / 5000đ, Increment=15p / 3000đ, GracePeriod=10p
    ///
    /// Xe đỗ 68 phút (= 60 phút base + 8 phút phát sinh):
    ///   OverMinutes = 68 - 60 = 8 phút
    ///   8 phút &lt;= GracePeriod (10 phút) → KHÔNG tính block mới
    ///   → Phí = BasePrice = 5000đ
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldIgnoreOvertime_WhenWithinGracePeriod()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 3000m,
                WindowCap = null,
                GracePeriodMinutes = 10  // Thời gian ân hạn 10 phút (Scenario 4)
            }
        };

        // Xe vào 08:00, ra 09:08 → 68 phút (quá 8 phút, nhỏ hơn GracePeriod 10p)
        var checkIn = new DateTime(2024, 1, 15, 8, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 9, 8, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert: Không tính thêm block mới vì 8 phút <= GracePeriod 10 phút
        Assert.Equal(5000m, result.TotalFee);
        Assert.Single(result.Details);

        var detail = result.Details[0];
        Assert.Equal(5000m, detail.BaseCharge);
        Assert.Equal(0, detail.IncrementBlocks);  // KHÔNG có block phát sinh
        Assert.Equal(0m, detail.IncrementCharge);
        Assert.Equal(5000m, detail.RawFee);
        Assert.Equal(5000m, detail.CappedFee);
    }

    /// <summary>
    /// Bổ sung Scenario 4: Phần phát sinh > GracePeriod thì VẪN tính block mới.
    ///
    /// Xe đỗ 82 phút (= 60 phút base + 22 phút phát sinh):
    ///   GracePeriod = 10 phút
    ///   OverMinutes = 22 phút > 10 phút → CÓ tính block mới
    ///   BillableOver = 22 - 10 = 12 phút
    ///   IncrementBlocks = Ceiling(12/15) = 1 block → IncrementCharge = 3000đ
    ///   → Phí = 5000 + 3000 = 8000đ
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldChargeIncrementBlock_WhenOvertimeExceedsGracePeriod()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Day Time Window",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 3000m,
                WindowCap = null,
                GracePeriodMinutes = 10  // Thời gian ân hạn 10 phút
            }
        };

        // Xe vào 08:00, ra 09:22 → 82 phút (22 phút vượt BaseDuration > GracePeriod 10p)
        var checkIn = new DateTime(2024, 1, 15, 8, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 9, 22, 0);

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert: 22 phút vượt > GracePeriod 10p → tính block
        // BillableOver = 22-10 = 12 phút → Ceiling(12/15) = 1 block
        Assert.Equal(8000m, result.TotalFee);

        var detail = result.Details[0];
        Assert.Equal(5000m, detail.BaseCharge);
        Assert.Equal(1, detail.IncrementBlocks);
        Assert.Equal(3000m, detail.IncrementCharge);
        Assert.Equal(8000m, detail.RawFee);
    }

    // =========================================================================
    // Edge Cases
    // =========================================================================

    /// <summary>
    /// checkOut &lt;= checkIn → phí = 0 (không tính giờ âm hay bằng 0).
    /// </summary>
    [Fact]
    public void CalculateFeeFromWindows_ShouldReturnZeroFee_WhenCheckOutEqualsOrBeforeCheckIn()
    {
        // Arrange
        var windows = new List<PricingWindow>
        {
            new PricingWindow
            {
                Id = 1,
                WindowName = "Test Window",
                StartTime = TimeSpan.Zero,
                EndTime = new TimeSpan(24, 0, 0),
                BaseDurationMinutes = 60,
                BasePrice = 5000m,
                IncrementBlockMinutes = 15,
                IncrementPrice = 2000m,
                WindowCap = null,
                GracePeriodMinutes = 0
            }
        };

        var checkIn = new DateTime(2024, 1, 15, 10, 0, 0);
        var checkOut = checkIn; // Cùng thời điểm

        // Act
        var result = _feeCalculationService.CalculateFeeFromWindows(windows, checkIn, checkOut);

        // Assert
        Assert.Equal(0m, result.TotalFee);
        Assert.Empty(result.Details);
    }

    /// <summary>
    /// CalculateFeeAsync throw DomainException khi không tìm thấy PricingPolicy Active.
    /// </summary>
    [Fact]
    public async Task CalculateFeeAsync_ShouldThrowDomainException_WhenNoPolicyFound()
    {
        // Arrange
        int vehicleTypeId = 1;
        var checkIn = new DateTime(2024, 1, 15, 10, 0, 0);
        var checkOut = new DateTime(2024, 1, 15, 11, 0, 0);

        // Mock: không tìm thấy policy
        _policyRepositoryMock
            .GetActivePolicyAsync(vehicleTypeId, checkIn)
            .Returns((PricingPolicy?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _feeCalculationService.CalculateFeeAsync(vehicleTypeId, checkIn, checkOut)
        );

        Assert.Equal("PRICING_POLICY_NOT_FOUND", exception.ErrorCode);
    }
}

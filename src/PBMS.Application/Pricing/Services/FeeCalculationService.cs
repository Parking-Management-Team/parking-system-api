using PBMS.Application.Contracts;
using PBMS.Application.Pricing.DTOs;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Pricing.Services;

/// <summary>
/// Triển khai dịch vụ tính toán phí gửi xe (IFeeCalculationService).
///
/// Business Rules được áp dụng:
///   BR-FEE-001 : Phí phụ thuộc vào loại xe → tra cứu PricingPolicy theo vehicle_type.
///   BR-FEE-002 : Tính từ check-in đến check-out.
///   BR-FEE-003 : Mô hình Pricing Window — mỗi khung giờ có bảng giá riêng.
///   BR-FEE-004 : Nếu thời gian trong BaseDuration → áp BasePrice.
///   BR-FEE-005 : Sau BaseDuration → tính thêm theo IncrementBlock.
///   BR-FEE-006 : Session qua nhiều window → tách theo từng window.
///   BR-FEE-007 : WindowCap chỉ giới hạn từng window, không giới hạn toàn session.
///   BR-FEE-008 : Hệ thống 24/7, không reset khi qua ngày.
///   BR-FEE-011 : Phát sinh <= GracePeriod → không tính block mới (Scenario 4).
///   BR-FEE-012 : Phát sinh > GracePeriod → tính thành block mới.
/// </summary>
public class FeeCalculationService : IFeeCalculationService
{
    private readonly IPricingPolicyRepository _pricingPolicyRepository;

    /// <summary>
    /// Constructor nhận IPricingPolicyRepository qua Dependency Injection.
    /// </summary>
    public FeeCalculationService(IPricingPolicyRepository pricingPolicyRepository)
    {
        _pricingPolicyRepository = pricingPolicyRepository
            ?? throw new ArgumentNullException(nameof(pricingPolicyRepository));
    }

    // -----------------------------------------------------------------------
    // PUBLIC: CalculateFeeAsync — Tra cứu policy từ DB rồi tính phí
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tính phí gửi xe bằng cách:
    ///   1. Tìm PricingPolicy Active cho loại xe tại thời điểm check-in.
    ///   2. Gọi CalculateFeeFromWindows với danh sách PricingWindow.
    ///   3. LẤY PARKING SESSION ĐỂ TÌM RA vehicleTypeId & checkIn
    /// </summary>
    public async Task<FeeCalculationResult> CalculateFeeAsync(
        int vehicleTypeId,
        DateTime checkIn,
        DateTime checkOut)
    {
        // Bước 1: Tra cứu chính sách giá đang áp dụng cho loại xe tại check-in
        var policy = await _pricingPolicyRepository.GetActivePolicyAsync(vehicleTypeId, checkIn);

        if (policy == null)
        {
            throw new DomainException(
                errorCode: "PRICING_POLICY_NOT_FOUND",
                message: $"No active pricing policy found for vehicle type ID={vehicleTypeId} " +
                         $"at {checkIn:dd/MM/yyyy HH:mm:ss}."
            );
        }

        if (!policy.PricingWindows.Any())
        {
            throw new DomainException(
                errorCode: "PRICING_WINDOWS_REQUIRED",
                message: $"Pricing policy with ID={policy.Id} has no windows configured."
            );
        }

        // Bước 2: Tính phí từ danh sách PricingWindow đã load
        return CalculateFeeFromWindows(policy.PricingWindows, checkIn, checkOut);
    }

    // -----------------------------------------------------------------------
    // PUBLIC: CalculateFeeFromWindows — Core logic (testable, không phụ thuộc DB)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tính phí trực tiếp từ danh sách PricingWindows.
    /// Đây là core logic có thể test độc lập không cần DB.
    ///
    /// Thuật toán:
    ///   1. Sắp xếp windows theo StartTime.
    ///   2. Phân tách [checkIn, checkOut] thành các đoạn thời gian theo window cycle (24h).
    ///   3. Với mỗi đoạn thời gian: tìm window phù hợp → tính phí.
    ///   4. Cộng tổng tất cả các đoạn.
    ///
    /// Xử lý 24/7 (BR-FEE-008): Mỗi ngày lặp lại chu kỳ window của ngày đó,
    ///   session không bị reset khi qua ngày — thời gian đỗ được tính liên tục.
    /// </summary>
    public FeeCalculationResult CalculateFeeFromWindows(
        IEnumerable<PricingWindow> windows,
        DateTime checkIn,
        DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            return new FeeCalculationResult { TotalFee = 0, Details = new() };
        }

        var windowList = windows.OrderBy(w => w.StartTime).ToList();
        var details = new List<WindowFeeDetail>();
        decimal totalFee = 0m;

        // --- Phân tách khoảng thời gian [checkIn, checkOut] thành các đoạn theo window ---
        // Mỗi đoạn là phần giao của [checkIn, checkOut] với một "lần xuất hiện" của window.
        //
        // Ví dụ: checkIn = 21:00, checkOut = hôm sau 07:30
        //   - Window đêm   22:00-06:00 → đoạn [21:00–22:00] thuộc window ngày hôm trước
        //   - Window ngày  06:00-22:00 → đoạn [06:00–07:30] thuộc window ngày hôm sau
        //   - v.v.

        var segments = SplitTimeIntoWindowSegments(windowList, checkIn, checkOut);

        foreach (var seg in segments)
        {
            var detail = CalculateWindowSegmentFee(seg.Window, seg.SegmentStart, seg.SegmentEnd);
            details.Add(detail);
            totalFee += detail.CappedFee;
        }

        return new FeeCalculationResult
        {
            TotalFee = totalFee,
            Details = details
        };
    }

    // -----------------------------------------------------------------------
    // PRIVATE: SplitTimeIntoWindowSegments
    // -----------------------------------------------------------------------

    /// <summary>
    /// Phân tách khoảng [checkIn, checkOut] thành các đoạn nhỏ,
    /// mỗi đoạn thuộc một PricingWindow cụ thể.
    ///
    /// Xử lý 24/7 (BR-FEE-008): duyệt từng phút trong ngày theo chu kỳ.
    /// Nếu windows bao phủ 24 giờ (tổng end-start = 24h), mọi phút đều được phủ.
    /// Nếu có khe hở (không có window bao phủ) thì phút đó không phát sinh phí.
    /// </summary>
    private static List<(PricingWindow Window, DateTime SegmentStart, DateTime SegmentEnd)>
        SplitTimeIntoWindowSegments(
            List<PricingWindow> windows,
            DateTime checkIn,
            DateTime checkOut)
    {
        var result = new List<(PricingWindow Window, DateTime SegmentStart, DateTime SegmentEnd)>();

        // Duyệt từng ngày trong khoảng thời gian gửi xe
        // Ngày bắt đầu là ngày của checkIn, kết thúc khi đã quét hết checkOut
        var currentDayStart = checkIn.Date; // 00:00:00 của ngày check-in

        while (currentDayStart < checkOut)
        {
            // Với mỗi window, tính phần giao với [checkIn, checkOut] trong ngày này
            foreach (var window in windows)
            {
                // Tính thời điểm window bắt đầu và kết thúc trong ngày hiện tại
                DateTime windowDayStart = currentDayStart + window.StartTime;
                DateTime windowDayEnd;

                // Xử lý window qua ngày (ví dụ: StartTime=22:00, EndTime=06:00)
                if (window.EndTime <= window.StartTime)
                {
                    // Window vắt qua ngày: end thuộc ngày hôm sau
                    windowDayEnd = currentDayStart.AddDays(1) + window.EndTime;
                }
                else
                {
                    windowDayEnd = currentDayStart + window.EndTime;
                }

                // Tính phần giao [segStart, segEnd] = intersection([checkIn, checkOut], [windowStart, windowEnd])
                DateTime segStart = Max(checkIn, windowDayStart);
                DateTime segEnd = Min(checkOut, windowDayEnd);

                // Chỉ thêm vào nếu có đoạn thực sự (segStart < segEnd)
                if (segStart < segEnd)
                {
                    result.Add((window, segStart, segEnd));
                }
            }

            // Chuyển sang ngày tiếp theo
            currentDayStart = currentDayStart.AddDays(1);
        }

        // Sắp xếp các đoạn theo thứ tự thời gian để chi tiết dễ đọc
        result.Sort((a, b) => a.SegmentStart.CompareTo(b.SegmentStart));

        return result;
    }

    // -----------------------------------------------------------------------
    // PRIVATE: CalculateWindowSegmentFee
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tính phí cho một đoạn thời gian [segmentStart, segmentEnd] theo quy tắc của PricingWindow.
    ///
    /// Logic:
    ///   1. Tính totalMinutes = (segmentEnd - segmentStart).TotalMinutes.
    ///   2. Nếu totalMinutes &lt;= BaseDurationMinutes → phí = BasePrice, không có block phát sinh.
    ///   3. Nếu totalMinutes > BaseDurationMinutes:
    ///      - BaseCharge = BasePrice
    ///      - overMinutes = totalMinutes - BaseDurationMinutes
    ///      - [Scenario 4 / BR-FEE-011] Nếu overMinutes &lt;= GracePeriodMinutes → KHÔNG tính block mới.
    ///      - [BR-FEE-012] Nếu overMinutes > GracePeriodMinutes:
    ///          incrementBlocks = Ceiling((overMinutes - GracePeriodMinutes) / IncrementBlockMinutes)
    ///          IncrementCharge = incrementBlocks × IncrementPrice
    ///   4. RawFee = BaseCharge + IncrementCharge
    ///   5. [Scenario 3 / BR-FEE-007] CappedFee = Min(RawFee, WindowCap) nếu WindowCap != null.
    /// </summary>
    private static WindowFeeDetail CalculateWindowSegmentFee(
        PricingWindow window,
        DateTime segmentStart,
        DateTime segmentEnd)
    {
        var totalMinutes = (segmentEnd - segmentStart).TotalMinutes;

        decimal baseCharge = 0m;
        int incrementBlocks = 0;
        decimal incrementCharge = 0m;

        if (totalMinutes <= 0)
        {
            // Đoạn rỗng → phí = 0
            return new WindowFeeDetail
            {
                PricingWindowId = window.Id,
                WindowName = window.WindowName,
                SegmentStart = segmentStart,
                SegmentEnd = segmentEnd,
                SegmentMinutes = totalMinutes,
                BaseCharge = 0m,
                IncrementBlocks = 0,
                IncrementCharge = 0m,
                RawFee = 0m,
                CappedFee = 0m
            };
        }

        if (totalMinutes <= window.BaseDurationMinutes)
        {
            // [BR-FEE-004] Thời gian nằm trong BaseDuration → áp BasePrice
            baseCharge = window.BasePrice;
        }
        else
        {
            // [BR-FEE-005] Vượt BaseDuration → tính thêm block lũy tiến
            baseCharge = window.BasePrice;

            double overMinutes = totalMinutes - window.BaseDurationMinutes;

            // [Scenario 4 / BR-FEE-011] Áp dụng thời gian ân hạn
            // Nếu phần vượt quá <= GracePeriodMinutes → KHÔNG tính block mới
            if (overMinutes > window.GracePeriodMinutes)
            {
                // [BR-FEE-012] Phần vượt quá > grace → tính block mới
                double billableOverMinutes = overMinutes - window.GracePeriodMinutes;
                incrementBlocks = (int)Math.Ceiling(billableOverMinutes / window.IncrementBlockMinutes);
                incrementCharge = incrementBlocks * window.IncrementPrice;
            }
            // else: overMinutes <= GracePeriodMinutes → incrementBlocks = 0, incrementCharge = 0
        }

        decimal rawFee = baseCharge + incrementCharge;

        // [Scenario 3 / BR-FEE-007] Áp WindowCap — chỉ giới hạn trong từng window
        decimal cappedFee = rawFee;
        if (window.WindowCap.HasValue && rawFee > window.WindowCap.Value)
        {
            cappedFee = window.WindowCap.Value;
        }

        return new WindowFeeDetail
        {
            PricingWindowId = window.Id,
            WindowName = window.WindowName,
            SegmentStart = segmentStart,
            SegmentEnd = segmentEnd,
            SegmentMinutes = totalMinutes,
            BaseCharge = baseCharge,
            IncrementBlocks = incrementBlocks,
            IncrementCharge = incrementCharge,
            RawFee = rawFee,
            CappedFee = cappedFee
        };
    }

    // -----------------------------------------------------------------------
    // PRIVATE: Helper methods
    // -----------------------------------------------------------------------

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}

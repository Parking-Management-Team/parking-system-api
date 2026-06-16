namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// Kết quả tính phí gửi xe cho một lượt xe (ParkingSession).
/// Chứa tổng phí và chi tiết phân tách theo từng khung giờ.
/// </summary>
public class FeeCalculationResult
{
    /// <summary>
    /// Tổng phí cần thanh toán (đã áp dụng WindowCap và GracePeriod).
    /// </summary>
    public decimal TotalFee { get; set; }

    /// <summary>
    /// Chi tiết phí của từng đoạn thời gian trong mỗi khung giờ.
    /// Dùng cho audit / hiển thị breakdown cho khách hàng.
    /// </summary>
    public List<WindowFeeDetail> Details { get; set; } = new();
}

/// <summary>
/// Chi tiết phí của một đoạn thời gian nằm trong một PricingWindow.
/// Theo BR-FEE-006: Nếu session đi qua nhiều pricing window, hệ thống tách session theo từng window.
/// </summary>
public class WindowFeeDetail
{
    /// <summary>ID của PricingWindow áp dụng.</summary>
    public int PricingWindowId { get; set; }

    /// <summary>Tên khung giờ (để hiển thị).</summary>
    public string WindowName { get; set; } = null!;

    /// <summary>Thời điểm bắt đầu đoạn thời gian trong khung giờ này.</summary>
    public DateTime SegmentStart { get; set; }

    /// <summary>Thời điểm kết thúc đoạn thời gian trong khung giờ này.</summary>
    public DateTime SegmentEnd { get; set; }

    /// <summary>Tổng phút của đoạn này.</summary>
    public double SegmentMinutes { get; set; }

    /// <summary>Phí block cơ bản (BasePrice) của đoạn này.</summary>
    public decimal BaseCharge { get; set; }

    /// <summary>Số block lũy tiến phát sinh.</summary>
    public int IncrementBlocks { get; set; }

    /// <summary>Phí lũy tiến phát sinh (IncrementBlocks × IncrementPrice).</summary>
    public decimal IncrementCharge { get; set; }

    /// <summary>Phí gốc trước khi áp WindowCap.</summary>
    public decimal RawFee { get; set; }

    /// <summary>
    /// Phí sau khi áp WindowCap (= Min(RawFee, WindowCap) nếu WindowCap có giá trị).
    /// Theo Scenario 3 / BR-FEE-007: WindowCap chỉ giới hạn trong từng window.
    /// </summary>
    public decimal CappedFee { get; set; }
}

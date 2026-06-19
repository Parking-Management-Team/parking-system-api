namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Khung giờ giá (PricingWindow) lưu luật tính phí chi tiết theo khung giờ trong ngày.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class PricingWindow : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới chính sách giá cha (PricingPolicy).
    /// </summary>
    public int PricingPolicyId { get; set; }

    /// <summary>
    /// Tên khung giờ (Ví dụ: "Khung giờ ngày", "Khung giờ đêm").
    /// </summary>
    public string WindowName { get; set; } = null!;

    /// <summary>
    /// Giờ bắt đầu khung giờ (Ví dụ: 06:00:00).
    /// Dùng TimeSpan trong .NET để tương thích kiểu TIME của PostgreSQL.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc khung giờ (Ví dụ: 22:00:00).
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Thời lượng cơ bản của block đầu tiên (Tính bằng phút, ví dụ: 60 phút).
    /// </summary>
    public int BaseDurationMinutes { get; set; }

    /// <summary>
    /// Giá của block thời lượng cơ bản đầu tiên.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Kích thước block thời gian phát sinh tiếp theo (Tính bằng phút, ví dụ: 15 phút).
    /// </summary>
    public int IncrementBlockMinutes { get; set; }

    /// <summary>
    /// Giá tiền của mỗi block phát sinh tiếp theo.
    /// </summary>
    public decimal IncrementPrice { get; set; }

    /// <summary>
    /// Mức giá trần tối đa trong khung giờ này (nullable nếu không áp dụng giới hạn).
    /// </summary>
    public decimal? WindowCap { get; set; }

    /// <summary>
    /// Thời gian ân hạn sau khi check-in hoặc thanh toán (Tính bằng phút, mặc định là 0).
    /// </summary>
    public int GracePeriodMinutes { get; set; } = 0;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin chính sách giá cha (PricingPolicy) chứa khung giờ này.
    /// </summary>
    public virtual PricingPolicy PricingPolicy { get; set; } = null!;
}
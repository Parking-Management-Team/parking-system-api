namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// DTO trả về thông tin chi tiết của một Khung giờ tính giá (PricingWindow).
/// </summary>
public class PricingWindowDto
{
    /// <summary>ID khung giờ.</summary>
    public int Id { get; set; }

    /// <summary>ID chính sách giá cha.</summary>
    public int PricingPolicyId { get; set; }

    /// <summary>Tên khung giờ (ví dụ: "Khung giờ ngày").</summary>
    public string WindowName { get; set; } = null!;

    /// <summary>Giờ bắt đầu khung giờ (HH:mm:ss).</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Giờ kết thúc khung giờ (HH:mm:ss).</summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>Thời lượng cơ bản của block đầu tiên (phút).</summary>
    public int BaseDurationMinutes { get; set; }

    /// <summary>Giá của block cơ bản đầu tiên.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Kích thước block lũy tiến (phút).</summary>
    public int IncrementBlockMinutes { get; set; }

    /// <summary>Giá mỗi block lũy tiến phát sinh.</summary>
    public decimal IncrementPrice { get; set; }

    /// <summary>Mức giá trần tối đa của khung giờ (null nếu không giới hạn).</summary>
    public decimal? WindowCap { get; set; }

    /// <summary>Thời gian ân hạn không tính block mới (phút).</summary>
    public int GracePeriodMinutes { get; set; }

    /// <summary>Thời điểm tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; }
}

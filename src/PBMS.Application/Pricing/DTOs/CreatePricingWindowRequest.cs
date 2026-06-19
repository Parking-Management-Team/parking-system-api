using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// Request body để tạo mới một Khung giờ tính giá (PricingWindow).
/// Áp dụng theo Scenario 1: Manager nhập đầy đủ tham số cấu hình.
/// </summary>
public class CreatePricingWindowRequest
{
    /// <summary>
    /// Tên khung giờ (ví dụ: "Khung giờ ngày").
    /// Tối đa 50 ký tự.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WindowName { get; set; } = null!;

    /// <summary>
    /// Giờ bắt đầu của khung giờ (HH:mm:ss).
    /// Ví dụ: "06:00:00".
    /// </summary>
    [Required]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc của khung giờ (HH:mm:ss).
    /// Ví dụ: "22:00:00".
    /// </summary>
    [Required]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Thời lượng cơ bản của block đầu tiên (phút, > 0).
    /// Ví dụ: 60 phút.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "BaseDurationMinutes must be greater than 0.")]
    public int BaseDurationMinutes { get; set; }

    /// <summary>
    /// Giá của block cơ bản đầu tiên (>= 0).
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "BasePrice must be greater than or equal to 0.")]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Kích thước block lũy tiến phát sinh (phút, > 0).
    /// Ví dụ: 15 phút.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "IncrementBlockMinutes must be greater than 0.")]
    public int IncrementBlockMinutes { get; set; }

    /// <summary>
    /// Giá mỗi block lũy tiến phát sinh (>= 0).
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "IncrementPrice must be greater than or equal to 0.")]
    public decimal IncrementPrice { get; set; }

    /// <summary>
    /// Mức giá trần tối đa của khung giờ (tùy chọn, null nếu không giới hạn).
    /// Nếu có giá trị thì phải >= BasePrice (Scenario 3).
    /// </summary>
    public decimal? WindowCap { get; set; }

    /// <summary>
    /// Thời gian ân hạn không tính block mới (phút, >= 0, mặc định 0).
    /// Scenario 4: Nếu phần phát sinh <= GracePeriodMinutes thì không tính thêm tiền.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "GracePeriodMinutes must be greater than or equal to 0.")]
    public int GracePeriodMinutes { get; set; } = 0;
}

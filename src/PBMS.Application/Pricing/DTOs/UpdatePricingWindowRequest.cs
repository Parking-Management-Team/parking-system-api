using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Pricing.DTOs;

/// <summary>
/// Request body để cập nhật thông tin một Khung giờ tính giá (PricingWindow).
/// Tất cả các trường đều tùy chọn (partial update).
/// </summary>
public class UpdatePricingWindowRequest
{
    /// <summary>
    /// Tên khung giờ mới (null nếu không thay đổi).
    /// </summary>
    [MaxLength(50)]
    public string? WindowName { get; set; }

    /// <summary>
    /// Giờ bắt đầu mới (null nếu không thay đổi).
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc mới (null nếu không thay đổi).
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Thời lượng cơ bản mới (phút, null nếu không thay đổi).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "BaseDurationMinutes phải lớn hơn 0.")]
    public int? BaseDurationMinutes { get; set; }

    /// <summary>
    /// Giá cơ bản mới (null nếu không thay đổi).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "BasePrice phải >= 0.")]
    public decimal? BasePrice { get; set; }

    /// <summary>
    /// Kích thước block lũy tiến mới (phút, null nếu không thay đổi).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IncrementBlockMinutes phải lớn hơn 0.")]
    public int? IncrementBlockMinutes { get; set; }

    /// <summary>
    /// Giá mỗi block lũy tiến mới (null nếu không thay đổi).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "IncrementPrice phải >= 0.")]
    public decimal? IncrementPrice { get; set; }

    /// <summary>
    /// Mức giá trần mới của khung giờ (null để giữ nguyên, 0 để bỏ giới hạn).
    /// </summary>
    public decimal? WindowCap { get; set; }

    /// <summary>
    /// Cờ xóa WindowCap (true thì set về null, false hoặc null thì dùng WindowCap nếu có).
    /// </summary>
    public bool RemoveWindowCap { get; set; } = false;

    /// <summary>
    /// Thời gian ân hạn mới (phút, null nếu không thay đổi).
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "GracePeriodMinutes phải >= 0.")]
    public int? GracePeriodMinutes { get; set; }
}

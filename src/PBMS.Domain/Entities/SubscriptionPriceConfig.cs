namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể cấu hình giá vé tháng (SubscriptionPriceConfig).
/// Lưu trữ lịch sử bảng giá vé tháng để đảm bảo tính minh bạch tài chính.
/// Kế thừa từ BaseEntity và ISoftDeletable.
/// </summary>
public class SubscriptionPriceConfig : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện áp dụng (VehicleType).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Mức giá của gói vé tháng.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Thời điểm bắt đầu áp dụng mức giá này.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Thời điểm kết thúc áp dụng mức giá này (có thể null nếu đang áp dụng vô thời hạn).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Đánh dấu xem cấu hình giá này có đang được kích hoạt hay không.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // -----------------------------------------------------------------------
    // SOFT DELETE PROPERTIES
    // -----------------------------------------------------------------------
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin loại phương tiện được áp dụng mức giá này.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Danh sách các đăng ký vé tháng áp dụng mức giá này.
    /// </summary>
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();
}

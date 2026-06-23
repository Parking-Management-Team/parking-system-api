namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Cấu hình giá phạt sự cố (PenaltyConfig).
/// Lưu trữ lịch sử bảng giá phạt cho từng loại sự cố để đảm bảo tính minh bạch.
/// Kế thừa từ BaseEntity và ISoftDeletable.
/// </summary>
public class PenaltyConfig : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// Khóa ngoại liên kết tới loại sự cố (IncidentType).
    /// </summary>
    public int IncidentTypeId { get; set; }

    /// <summary>
    /// Số tiền phạt áp dụng cho sự cố này.
    /// </summary>
    public decimal PenaltyFee { get; set; }

    /// <summary>
    /// Thời điểm bắt đầu áp dụng mức giá phạt này.
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Thời điểm kết thúc áp dụng mức giá phạt này (có thể null nếu đang áp dụng vô thời hạn).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Đánh dấu xem cấu hình giá phạt này có đang được kích hoạt hay không.
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
    /// Thông tin loại sự cố được áp dụng mức giá này.
    /// </summary>
    public virtual IncidentType IncidentType { get; set; } = null!;

    /// <summary>
    /// Danh sách các sự cố thực tế đã áp dụng mức giá phạt này.
    /// </summary>
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}

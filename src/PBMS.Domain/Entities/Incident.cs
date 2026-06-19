using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Sự cố (Incident) lưu trữ thông tin về các sự cố phát sinh trong lượt gửi xe.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.14 — Physical Model: incident
/// </summary>
public class Incident : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// Khóa ngoại liên kết tới lượt gửi xe xảy ra sự cố (ParkingSession).
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới danh mục loại sự cố (IncidentType).
    /// </summary>
    public int IncidentTypeId { get; set; }

    /// <summary>
    /// Mô tả chi tiết thực tế của sự cố cụ thể này.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Số tiền phạt thực tế áp dụng cho sự cố này (có thể điều chỉnh từ DefaultPenaltyFee của IncidentType).
    /// </summary>
    public decimal? PenaltyFee { get; set; }

    /// <summary>
    /// Trạng thái xử lý sự cố.
    /// Tham chiếu SRS: §8.3.3.14 — incident_status
    /// </summary>
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    /// <summary>
    /// Thời điểm sự cố được xử lý xong xuôi.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

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
    /// Lượt gửi xe (ParkingSession) ghi nhận sự cố này.
    /// </summary>
    public virtual ParkingSession Session { get; set; } = null!;

    /// <summary>
    /// Loại sự cố (IncidentType) được phân loại.
    /// </summary>
    public virtual IncidentType IncidentType { get; set; } = null!;

    /// <summary>
    /// Danh sách các xe/thẻ bị khóa hoặc đưa vào danh sách đen (Blacklist) do sự cố này gây ra.
    /// </summary>
    public virtual ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();
}

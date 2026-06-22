namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Loại sự cố (IncidentType) phân loại các sự cố xảy ra trong hệ thống bãi xe (Ví dụ: Mất thẻ, Hỏng hóc).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.13 — Physical Model: incident_type
/// </summary>
public class IncidentType : BaseEntity
{
    /// <summary>
    /// Mã loại sự cố duy nhất (Ví dụ: "LOST_CARD", "WRONG_LANE").
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(20).
    /// </summary>
    public string IncidentCode { get; set; } = null!;

    /// <summary>
    /// Tên hiển thị loại sự cố (Ví dụ: "Mất thẻ gửi xe", "Đi sai làn đường").
    /// </summary>
    public string IncidentName { get; set; } = null!;

    /// <summary>
    /// Mô tả chi tiết về loại sự cố này.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Danh sách các sự cố (Incident) thực tế thuộc loại này.
    /// </summary>
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    /// <summary>
    /// Danh sách các cấu hình giá phạt áp dụng cho loại sự cố này theo thời gian.
    /// </summary>
    public virtual ICollection<PenaltyConfig> PenaltyConfigs { get; set; } = new List<PenaltyConfig>();
}

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Danh sách đen (Blacklist) dùng để chặn phương tiện hoặc thẻ gửi xe vi phạm.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.15 — Physical Model: blacklist
/// </summary>
public class Blacklist : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới xe bị chặn (Vehicle).
    /// Cho phép Null nếu chặn theo Thẻ gửi xe.
    /// </summary>
    public int? VehicleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới thẻ gửi xe bị chặn (Card).
    /// Cho phép Null nếu chặn theo Xe.
    /// </summary>
    public int? CardId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới sự cố cụ thể dẫn tới việc bị đưa vào danh sách đen (Incident).
    /// Cho phép Null nếu bị đưa vào danh sách đen không qua báo cáo sự cố (ví dụ: chặn thủ công).
    /// </summary>
    public int? IncidentId { get; set; }

    /// <summary>
    /// Lý do đưa xe, thẻ hoặc sự cố này vào danh sách chặn.
    /// </summary>
    public string Reason { get; set; } = null!;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin phương tiện bị chặn.
    /// </summary>
    public virtual Vehicle? Vehicle { get; set; }

    /// <summary>
    /// Thông tin thẻ gửi xe bị chặn.
    /// </summary>
    public virtual Card? Card { get; set; }

    /// <summary>
    /// Thông tin sự cố gây ra việc chặn.
    /// </summary>
    public virtual Incident? Incident { get; set; }
}

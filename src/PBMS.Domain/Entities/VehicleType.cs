namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Loại phương tiện (Vehicle Type).
/// Ví dụ: "Xe máy", "Ô tô".
/// Tham chiếu SRS: §8.3.3.9 — Physical Model: vehicle_type
/// </summary>
public class VehicleType : BaseEntity
{
    /// <summary>
    /// Tên loại phương tiện.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Mô tả về loại phương tiện.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái của loại phương tiện.
    /// </summary>
    public string Status { get; set; } = "Active";
}

using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Tầng (Floor) thuộc một tòa nhà gửi xe.
/// Quản lý thông tin về số tầng và các khu vực (Zone) bên trong tầng đó.
/// Tham chiếu SRS: §8.3.3, Table 3.6 — Physical Model: floor
/// </summary>
public class Floor : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tòa nhà (Building).
    /// </summary>
    public int BuildingId { get; set; }

    /// <summary>
    /// Số tầng (Ví dụ: 1, 2, -1 cho hầm).
    /// Ràng buộc: UNIQUE cùng với BuildingId.
    /// </summary>
    public int FloorNumber { get; set; }

    /// <summary>
    /// Tên gợi nhớ của tầng (Ví dụ: "Hầm B1", "Tầng trệt").
    /// Độ dài tối đa: 50 ký tự. Cho phép NULL theo SRS.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tầng (Ví dụ: Active, OutOfService).
    /// </summary>
    public FloorStatus Status { get; set; } = FloorStatus.Active;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tòa nhà chứa tầng này.
    /// </summary>
    public virtual Building Building { get; set; } = null!;

    /// <summary>
    /// Danh sách các khu vực (Zone) thuộc tầng này.
    /// </summary>
    public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
}

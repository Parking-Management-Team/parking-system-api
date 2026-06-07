using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Khu vực (Zone) thuộc một tầng (Floor).
/// Mỗi khu vực phục vụ một loại phương tiện cụ thể và có sức chứa giới hạn.
/// Tham chiếu SRS: §8.3.3, Table 3.8 — Physical Model: zone
/// </summary>
public class Zone : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tầng (Floor).
    /// </summary>
    public int FloorId { get; set; }

    /// <summary>
    /// Mã khu vực (Ví dụ: "ZA", "VIP-01").
    /// Ràng buộc: UNIQUE cùng với FloorId.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Tên khu vực (Ví dụ: "Khu vực A", "Tầng 1 - Khu VIP").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện (VehicleType).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Sức chứa tối đa của khu vực.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của khu vực.
    /// </summary>
    public ZoneStatus Status { get; set; } = ZoneStatus.Available;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tầng chứa khu vực này.
    /// </summary>
    public virtual Floor Floor { get; set; } = null!;

    /// <summary>
    /// Thông tin loại phương tiện được phép đỗ tại khu vực này.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Danh sách các vị trí đỗ xe cụ thể (ParkingSlot) thuộc khu vực này.
    /// </summary>
    public virtual ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();
}

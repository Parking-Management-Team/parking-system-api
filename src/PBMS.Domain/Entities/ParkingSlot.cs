using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Vị trí đỗ xe cụ thể (Parking Slot).
/// Đặc biệt quan trọng với ô tô booking và ô tô thẻ tháng.
/// Tham chiếu SRS: §8.3.3.9 — Physical Model: parking_slot
/// </summary>
public class ParkingSlot : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới khu vực (Zone) chứa slot này.
    /// </summary>
    public int ZoneId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới loại phương tiện phù hợp (VehicleType).
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Mã định danh duy nhất của slot (Ví dụ: "SLOT-A01").
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(20).
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Tên hiển thị của slot (Ví dụ: "Vị trí A1").
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Trạng thái hiện tại của slot (Available, Reserved, Occupied, Blocked).
    /// </summary>
    public SlotStatus Status { get; set; } = SlotStatus.Available;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin khu vực (Zone) chứa slot này.
    /// </summary>
    public virtual Zone Zone { get; set; } = null!;

    /// <summary>
    /// Thông tin loại phương tiện phù hợp với slot này.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Danh sách các lượt gửi xe (ParkingSession) đã hoặc đang sử dụng slot này.
    /// </summary>
    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    /// <summary>
    /// Danh sách các đăng ký vé tháng (MonthlySubscription) được gán vị trí đỗ xe này.
    /// </summary>
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();

    /// <summary>
    /// Danh sách các đặt chỗ (Booking) đã hoặc đang liên kết với slot này.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

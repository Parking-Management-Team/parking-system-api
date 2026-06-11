namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Loại phương tiện (VehicleType) phân loại các xe trong hệ thống (Ví dụ: Xe máy, Ô tô).
/// Kế thừa từ BaseEntity (chứa Id, CreatedAt, RowVersion).
/// </summary>
public class VehicleType : BaseEntity
{
    /// <summary>
    /// Tên loại phương tiện (Ví dụ: "Motorcycle", "Car").
    /// Ràng buộc: UNIQUE, NOT NULL, varchar(50).
    /// </summary>
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// Mô tả chi tiết về loại phương tiện này.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của loại phương tiện (Ví dụ: "Active", "Inactive").
    /// </summary>
    public string VehicleTypeStatus { get; set; } = "Active";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Danh sách các vị trí đỗ xe (ParkingSlot) thuộc loại phương tiện này.
    /// </summary>
    public virtual ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();

    /// <summary>
    /// Danh sách các xe (Vehicle) thuộc loại phương tiện này.
    /// </summary>
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    /// <summary>
    /// Danh sách các chính sách giá (PricingPolicy) áp dụng cho loại phương tiện này.
    /// </summary>
    public virtual ICollection<PricingPolicy> PricingPolicies { get; set; } = new List<PricingPolicy>();

    /// <summary>
    /// Danh sách các đặt chỗ (Booking) thuộc loại phương tiện này.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

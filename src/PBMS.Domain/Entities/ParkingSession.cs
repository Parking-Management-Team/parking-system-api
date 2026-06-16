namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể lượt gửi xe (ParkingSession) — lưu toàn bộ vòng đời từ check-in đến check-out.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.12 — Physical Model: parking_session
/// </summary>
public class ParkingSession
{
    public int SessionId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới phương tiện gửi xe (Vehicle).
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới tòa nhà nơi session diễn ra (Building).
    /// </summary>
    public int BuildingId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Card được dùng nhận diện session.
    /// Bắt buộc trong scope hiện tại (NOT NULL).
    /// </summary>
    public int CardId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Zone được gợi ý hoặc sử dụng (nullable).
    /// Xe máy: zone_id phản ánh zone thực tế; ô tô: zone_id được gợi ý khi check-in.
    /// </summary>
    public int? ZoneId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Slot thực tế được dùng (nullable).
    /// Xe máy: luôn NULL; ô tô Walk-in/Booking: NULL cho đến khi xác nhận vị trí đậu;
    /// ô tô Monthly Subscription: bằng monthly_subscription.assigned_slot_id.
    /// </summary>
    public int? SlotId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Booking đã chuyển thành session (nullable, UNIQUE).
    /// Một booking chỉ được chuyển thành tối đa một session.
    /// booking_id và monthly_subscription_id không được đồng thời có giá trị.
    /// </summary>
    public int? BookingId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Monthly Subscription được áp dụng (nullable).
    /// booking_id và monthly_subscription_id không được đồng thời có giá trị.
    /// </summary>
    public int? MonthlySubscriptionId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Staff xử lý check-in (nullable).
    /// </summary>
    public int? InStaffId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Staff xử lý check-out (nullable).
    /// </summary>
    public int? OutStaffId { get; set; }

    /// <summary>
    /// Thời điểm check-in (xe vào bãi).
    /// Là mốc xác định Pricing Policy áp dụng (Vehicle Type + check_in_time).
    /// </summary>
    public DateTime CheckInTime { get; set; }

    /// <summary>
    /// Thời điểm check-out — ghi nhận ngay khi bắt đầu quy trình check-out.
    /// Dùng làm mốc kết thúc tính phí nếu cùng quá trình check-out tiếp tục.
    /// NULL nếu session vẫn đang mở.
    /// Bị hủy (NULL lại) nếu check-out FAILED.
    /// </summary>
    public DateTime? CheckOutTime { get; set; }

    /// <summary>
    /// Biển số xe lúc check-in (ghi nhận tại thời điểm vào bãi).
    /// </summary>
    public string LicensePlateIn { get; set; } = null!;

    /// <summary>
    /// Biển số xe lúc check-out (có thể được chỉnh sửa nếu có sai sót).
    /// </summary>
    public string? LicensePlateOut { get; set; }

    /// <summary>
    /// Trạng thái vòng đời lượt gửi xe.
    /// Giá trị: ACTIVE, COMPLETED, LOST, EXPIRED, DOWNGRADED
    /// </summary>
    public string SessionStatus { get; set; } = "ACTIVE";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>Phương tiện gửi xe.</summary>
    public virtual Vehicle Vehicle { get; set; } = null!;

    /// <summary>Tòa nhà nơi session diễn ra.</summary>
    public virtual Building Building { get; set; } = null!;

    /// <summary>Card được dùng nhận diện session.</summary>
    public virtual Card Card { get; set; } = null!;

    /// <summary>Zone được phân bổ (nullable).</summary>
    public virtual Zone? Zone { get; set; }

    /// <summary>Slot thực tế (nullable).</summary>
    public virtual ParkingSlot? ParkingSlot { get; set; }

    /// <summary>Booking đã chuyển thành session (nullable).</summary>
    public virtual Booking? Booking { get; set; }

    /// <summary>Monthly Subscription được áp dụng (nullable).</summary>
    public virtual MonthlySubscription? MonthlySubscription { get; set; }

    /// <summary>Staff check-in (nullable).</summary>
    public virtual Account? InStaff { get; set; }

    /// <summary>Staff check-out (nullable).</summary>
    public virtual Account? OutStaff { get; set; }

    /// <summary>Danh sách sự cố phát sinh trong session.</summary>
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    /// <summary>Danh sách giao dịch thanh toán từ session.</summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

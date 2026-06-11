namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể hồ sơ đăng ký vé tháng (Monthly Subscription).
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// Tham chiếu SRS: §8.3.3.17 — Physical Model: monthly_subscription
/// </summary>
public class MonthlySubscription : BaseEntity
{
    /// <summary>
    /// Khóa ngoại liên kết tới tài khoản đăng ký (Account).
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới xe được áp dụng quyền lợi vé tháng (Vehicle).
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới thẻ MONTHLY cấp cho tài xế (Card).
    /// Có thể Null khi hồ sơ đang ở trạng thái PENDING chờ kích hoạt.
    /// Ràng buộc: UNIQUE trong DB (Mỗi thẻ tại một thời điểm chỉ gán cho một vé tháng hoạt động).
    /// </summary>
    public int? AssignedCardId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới vị trí đỗ xe riêng của ô tô đăng ký tháng (ParkingSlot).
    /// Đối với xe máy, trường này bắt buộc là NULL.
    /// </summary>
    public int? AssignedSlotId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới tòa nhà/bãi đỗ xe áp dụng vé tháng (Building).
    /// </summary>
    public int BuildingId { get; set; }

    /// <summary>
    /// Giá tiền của gói đăng ký tháng tại thời điểm tạo hồ sơ.
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>
    /// Thời điểm bắt đầu kích hoạt quyền lợi vé tháng (Sau khi thanh toán thành công).
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// Thời điểm hết hiệu lực của vé tháng.
    /// </summary>
    public DateTime? ExpiredAt { get; set; }

    /// <summary>
    /// Trạng thái của hồ sơ đăng ký vé tháng.
    /// Các giá trị hợp lệ: "PENDING", "ACTIVE", "EXPIRED", "DOWNGRADED", "CANCELLED".
    /// </summary>
    public string MonthlySubscriptionStatus { get; set; } = "PENDING";

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thông tin tài khoản đăng ký.
    /// </summary>
    public virtual Account Account { get; set; } = null!;

    /// <summary>
    /// Thông tin xe áp dụng vé tháng.
    /// </summary>
    public virtual Vehicle Vehicle { get; set; } = null!;

    /// <summary>
    /// Thẻ gửi xe loại MONTHLY được gán cho vé tháng này.
    /// </summary>
    public virtual Card? AssignedCard { get; set; }

    /// <summary>
    /// Vị trí đỗ xe riêng được gán cho vé tháng này (chỉ áp dụng đối với ô tô).
    /// </summary>
    public virtual ParkingSlot? AssignedSlot { get; set; }

    /// <summary>
    /// Thông tin tòa nhà/bãi đỗ xe áp dụng.
    /// </summary>
    public virtual Building Building { get; set; } = null!;

    /// <summary>
    /// Danh sách các giao dịch thanh toán liên quan đến vé tháng này.
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

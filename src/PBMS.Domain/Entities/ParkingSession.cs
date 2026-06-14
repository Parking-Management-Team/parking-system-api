namespace PBMS.Domain.Entities;

/// <summary>
/// Represents a parking session - when a vehicle parks in the facility.
/// Thực thể Lượt gửi xe (ParkingSession) — đại diện cho một lần gửi xe từ check-in đến check-out.
///
/// ⚠️ FILE NÀY ĐANG Ở TRẠNG THÁI STUB (chỉ có properties cần thiết cho Card module).
///    Toàn bộ properties sẽ được implement trong task ParkingSession riêng.
///
/// Tham chiếu SRS: §8.3.3, Table 3.12 — Physical Model: parking_session
/// </summary>
public class ParkingSession : BaseEntity
{
    /// <summary>
    /// Khóa ngoại tới xe đang gửi.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết đến thẻ gửi xe (Card) được dùng trong lượt này.
    /// Nullable vì một số lượt gửi xe có thể không dùng thẻ vật lý.
    /// </summary>
    public int? CardId { get; set; }

    /// <summary>
    /// Trạng thái lượt gửi xe.
    /// Các giá trị: "Active", "Completed", "Lost", "Expired", "Downgraded"
    ///
    /// "Active" = xe đang trong bãi, chưa check-out
    /// </summary>
    public string SessionStatus { get; set; } = "Active";

    /// <summary>
    /// Cờ tương thích với repository hiện tại để xác định phiên đã hoàn tất.
    /// </summary>
    public bool IsCompleted { get; set; }

    // -----------------------------------------------------------------------
    // Navigation Properties — sẽ được bổ sung khi implement ParkingSession module
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thẻ gửi xe được dùng trong lượt này.
    /// virtual → EF Core Lazy Loading tự động load khi cần.
    /// </summary>
    public virtual Card? Card { get; set; }

    /// <summary>
    /// Xe thuộc lượt gửi này.
    /// </summary>
    public virtual Vehicle Vehicle { get; set; } = null!;

    /// <summary>
    /// Danh sách các giao dịch thanh toán (Payment) liên quan đến lượt gửi xe này.
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Danh sách các sự cố (Incident) phát sinh trong lượt gửi xe này.
    /// </summary>
    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}

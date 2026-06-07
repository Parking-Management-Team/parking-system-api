namespace PBMS.Domain.Entities;

/// <summary>
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

    // -----------------------------------------------------------------------
    // Navigation Properties — sẽ được bổ sung khi implement ParkingSession module
    // -----------------------------------------------------------------------

    /// <summary>
    /// Thẻ gửi xe được dùng trong lượt này.
    /// virtual → EF Core Lazy Loading tự động load khi cần.
    /// </summary>
    public virtual Card? Card { get; set; }
}
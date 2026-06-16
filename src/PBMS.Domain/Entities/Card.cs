using PBMS.Domain.Enums;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể Thẻ gửi xe (Card) — mô phỏng thẻ vật lý trong hệ thống PBMS.
///
/// Vì hệ thống không có phần cứng thật (không có thẻ vật lý, không có máy đọc thẻ),
/// Card ở đây đóng vai trò là "mã gửi xe mô phỏng" — tương đương tấm vé gửi xe
/// truyền thống nhưng được quản lý bằng phần mềm.
///
/// Cách hoạt động trong luồng nghiệp vụ:
///   1. Khi xe check-in: Staff chọn một Card có trạng thái Available,
///      gán vào ParkingSession → Card.CardStatus chuyển sang Active.
///   2. Khi xe check-out thành công: Card.CardStatus trả về Available để
///      dùng cho lượt gửi xe tiếp theo.
///   3. Nếu khách mất thẻ: Staff đổi CardStatus sang Lost,
///      hệ thống tự động cộng phí phạt lost_card_penalty vào tổng phí.
///
/// Tham chiếu SRS:
///   - §8.3.3, Table 3.11 — Physical Model: card
///   - §6.13 — Card Status rules
///   - §1.3  — Định nghĩa "Virtual Card Code"
///   - BR-052, BR-053 — Quy tắc xử lý mất thẻ
/// </summary>
public class Card : BaseEntity
{
    // -----------------------------------------------------------------------
    // THÔNG TIN ĐỊNH DANH THẺ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Mã thẻ nội bộ — dùng để tra cứu trong hệ thống.
    /// In ra vé/QR cho khách cầm khi gửi xe.
    ///
    /// Ràng buộc: UNIQUE — không có 2 thẻ nào có cùng CardCode.
    /// Độ dài tối đa: 20 ký tự (theo SRS varchar(20)).
    /// Ví dụ: "CARD-00123", "PKG-2026-001"
    /// </summary>
    public string CardCode { get; set; } = null!;

    /// <summary>
    /// Mã RFID mô phỏng — tuỳ chọn, dùng nếu muốn mô phỏng kịch bản quét thẻ.
    /// Trong phiên bản hiện tại hệ thống không có thiết bị đọc RFID thật,
    /// nên trường này có thể để null.
    ///
    /// Ràng buộc: UNIQUE khi có giá trị (nullable unique).
    /// Độ dài tối đa: 50 ký tự (theo SRS varchar(50)).
    /// </summary>
    public string? NfcUid { get; set; }

    // -----------------------------------------------------------------------
    // PHÂN LOẠI VÀ TRẠNG THÁI
    // -----------------------------------------------------------------------

    /// <summary>
    /// Loại thẻ — phân biệt các nhóm thẻ trong hệ thống.
    /// Ví dụ giá trị: "PARKING_CARD", "ACCESS_CARD"
    ///
    /// Hiện tại chủ yếu dùng "PARKING_CARD" cho nghiệp vụ gửi xe.
    /// Độ dài tối đa: 20 ký tự (theo SRS varchar(20)).
    /// </summary>
    public string CardType { get; set; } = null!;

    /// <summary>
    /// Trạng thái hiện tại của thẻ.
    ///
    /// Các giá trị hợp lệ (xem enum <see cref="CardStatus"/>):
    ///   - Available: thẻ rảnh, có thể gán cho session mới.
    ///   - Active   : thẻ đang được dùng trong một session đang mở.
    ///   - Lost     : thẻ bị báo mất — khoá lại và áp phí phạt.
    ///   - Blocked  : thẻ bị khoá thủ công bởi Manager/Admin.
    ///
    /// Lưu trong DB dưới dạng string (tên của enum) để dễ đọc khi query trực tiếp.
    /// Ví dụ: "Available", "Active", "Lost", "Blocked"
    /// </summary>
    public string CardStatus { get; set; } = Enums.CardStatus.Available.ToString();

    public DateTime? UpdatedAt { get; set; }

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES (Quan hệ với các bảng khác)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Danh sách các lượt gửi xe (ParkingSession) đã từng dùng thẻ này.
    ///
    /// Quan hệ: Một Card có thể được dùng nhiều lần qua nhiều phiên gửi xe,
    ///          nhưng tại một thời điểm chỉ được dùng cho TỐI ĐA một session ACTIVE.
    ///
    /// Từ khóa 'virtual' cho phép EF Core dùng Lazy Loading:
    /// khi gọi card.ParkingSessions, EF Core tự động truy vấn DB nếu chưa load sẵn.
    /// </summary>
    public virtual ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();

    /// <summary>
    /// Đăng ký vé tháng (MonthlySubscription) hiện tại đang được gán thẻ này.
    /// Quan hệ 1-1 (assigned_card_id là UNIQUE).
    /// </summary>
    public virtual MonthlySubscription? MonthlySubscription { get; set; }

    /// <summary>
    /// Danh sách các ghi nhận danh sách đen (Blacklist) liên quan đến thẻ này.
    /// </summary>
    public virtual ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();

    // -----------------------------------------------------------------------
    // HELPER PROPERTIES (Thuộc tính tiện ích — không lưu vào DB)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Kiểm tra nhanh xem thẻ có đang sẵn sàng để gán cho session mới không.
    /// Chỉ trả về true khi CardStatus == "Available".
    ///
    /// Đây là "computed property" — không được EF Core map vào cột DB
    /// (vì không có setter, EF Core tự hiểu và bỏ qua).
    /// </summary>
    public bool IsAvailable => CardStatus == Enums.CardStatus.Available.ToString();

    /// <summary>
    /// Kiểm tra nhanh xem thẻ có đang bị mất không.
    /// Dùng trong luồng Exception Handling khi Staff xử lý mất thẻ.
    /// Tham chiếu: BR-052, BR-053
    /// </summary>
    public bool IsLost => CardStatus == Enums.CardStatus.Lost.ToString();
}

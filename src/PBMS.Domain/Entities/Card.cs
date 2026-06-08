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
    /// Mã RFID của con chip trên thẻ vật lý — dùng để định danh và tra cứu thẻ.
    ///
    /// Trong thực tế, mỗi thẻ RFID vật lý có một con chip được nhà sản xuất gán
    /// sẵn một mã số duy nhất (UID). Đây chính là giá trị được lưu ở trường này.
    ///
    /// Ràng buộc: UNIQUE khi có giá trị (nullable unique).
    /// Độ dài tối đa: 50 ký tự (theo SRS varchar(50)).
    /// Để null nếu thẻ không có chip RFID hoặc chưa được gán mã.
    /// </summary>
    public string? RfidCode { get; set; }

    // -----------------------------------------------------------------------
    // LIÊN KẾT TÒA NHÀ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Khóa ngoại liên kết đến tòa nhà (Building) mà thẻ này thuộc về.
    /// Nullable — một thẻ có thể chưa được phân công cho tòa nhà cụ thể.
    /// </summary>
    public int? BuildingId { get; set; }

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
    ///   - Blocked  : thẻ bị khoá thủ công (Ngưng hoạt động / Inactive).
    ///
    /// Lưu trong DB dưới dạng string (tên của enum) để dễ đọc khi query trực tiếp.
    /// Ví dụ: "Available", "Active", "Lost", "Blocked"
    /// </summary>
    public string CardStatus { get; set; } = Enums.CardStatus.Available.ToString();

    /// <summary>
    /// Thời điểm Staff đánh dấu thẻ này là Đã mất (Lost).
    /// Null nếu thẻ chưa từng bị báo mất.
    ///
    /// Được set tự động bởi CardService.UpdateCardStatusAsync() khi chuyển sang Lost.
    /// Reset về null khi thẻ được mở lại (chuyển sang Available).
    ///
    /// Dùng để:
    ///   - Tính lost_card_penalty theo thời điểm báo mất (BR-052).
    ///   - Audit log — truy vết lịch sử báo mất thẻ.
    /// Tham chiếu SRS: BR-052, BR-053, Task con BE: "bổ sung ghi nhận thời điểm báo mất"
    /// </summary>
    public DateTime? LostAt { get; set; }

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

    /// <summary>
    /// Kiểm tra nhanh xem thẻ có đang bị khóa (Blocked / Ngưng hoạt động) không.
    /// Trả về true khi CardStatus == "Blocked".
    ///
    /// Dùng trong luồng check-in để chặn thẻ bị khóa thủ công.
    /// </summary>
    public bool IsBlocked => CardStatus == Enums.CardStatus.Blocked.ToString();
}
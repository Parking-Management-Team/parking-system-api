namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của thẻ gửi xe (Card) trong hệ thống PBMS.
///
/// Luồng chuyển trạng thái hợp lệ (State Machine):
///   AVAILABLE → ACTIVE    (System: thẻ được gán cho một lượt gửi xe khi check-in)
///   ACTIVE    → AVAILABLE (System: xe check-out thành công, thẻ được trả lại)
///   ACTIVE    → LOST      (Staff: khách báo mất thẻ trong khi xe đang gửi)
///   LOST      → AVAILABLE (Staff/Admin: xử lý xong sự cố mất thẻ)
///   AVAILABLE → BLOCKED   (Staff/Admin: khóa thẻ — Ngưng hoạt động thủ công)
///   BLOCKED   → AVAILABLE (Admin: mở khóa thẻ trở lại)
///
/// Lưu ý nghiệp vụ:
///   - "Blocked" (Ngưng hoạt động) là trạng thái Staff/Admin khóa thẻ thủ công.
///     Trong giao diện người dùng, trạng thái này hiển thị là "Inactive".
///   - Không cho phép chuyển từ AVAILABLE → LOST (thẻ phải đang Active mới báo mất).
///   - Không cho phép chuyển từ ACTIVE → BLOCKED (phải check-out trước).
///
/// Tham chiếu SRS: §6.13 System State Rules — Card Status
/// User Story: Card Status Management (Parking Staff)
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// Thẻ còn trống, chưa được gán cho lượt gửi xe nào.
    /// Đây là trạng thái mặc định khi thẻ mới được nhập vào hệ thống.
    /// Có thể chuyển sang: Active (check-in), Blocked (Staff khóa thủ công).
    /// </summary>
    Available,

    /// <summary>
    /// Thẻ đang được sử dụng trong một lượt gửi xe (parking session) đang mở.
    /// Một thẻ chỉ có thể Active cho tối đa một session cùng lúc.
    /// Có thể chuyển sang: Available (check-out), Lost (Staff báo mất).
    /// </summary>
    Active,

    /// <summary>
    /// Thẻ đã bị báo mất bởi Staff trong khi xe đang gửi (session đang Active).
    /// Khi Staff chuyển thẻ sang trạng thái này, hệ thống sẽ:
    ///   1. Ghi nhận thời điểm báo mất vào trường LostAt của Card.
    ///   2. Áp dụng phí phạt mất thẻ (lost_card_penalty) vào tổng phí của session.
    ///   3. Chỉ cho phép xe rời bãi sau khi khách thanh toán toàn bộ phí + phạt.
    ///   4. Thẻ bị chặn hoàn toàn — không thể dùng để check-in session mới.
    /// Có thể chuyển sang: Available (Staff/Admin xử lý xong sự cố).
    /// Tham chiếu SRS: BR-052, BR-053
    /// </summary>
    Lost,

    /// <summary>
    /// Thẻ bị khóa thủ công bởi Staff hoặc Admin — Ngưng hoạt động (Inactive).
    /// Áp dụng khi: thẻ bị hư hỏng, vi phạm, hoặc cần tạm ngừng sử dụng.
    /// Thẻ ở trạng thái này không thể dùng để thực hiện check-in xe vào bãi.
    /// Có thể chuyển sang: Available (Admin mở khóa trở lại).
    /// </summary>
    Blocked
}

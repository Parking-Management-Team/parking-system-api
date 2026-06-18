namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái của thẻ gửi xe (Card) trong hệ thống PBMS.
///
/// Luồng trạng thái điển hình:
///   AVAILABLE → ACTIVE    (khi thẻ được gán cho một lượt gửi xe / parking session)
///   ACTIVE    → AVAILABLE (khi xe check-out thành công, thẻ được trả lại)
///   ACTIVE    → LOST      (khi khách báo mất thẻ — Staff đánh dấu thủ công)
///   AVAILABLE → BLOCKED   (khi Admin/Manager khoá thẻ do hư hỏng hoặc vi phạm)
///
/// Tham chiếu SRS: §6.13 System State Rules — Card Status
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// Thẻ còn trống, chưa được gán cho lượt gửi xe nào.
    /// Đây là trạng thái mặc định khi thẻ mới được nhập vào hệ thống.
    /// </summary>
    Available,

    /// <summary>
    /// Thẻ đang được sử dụng trong một lượt gửi xe (parking session) đang mở.
    /// Một thẻ chỉ có thể ACTIVE cho tối đa một session cùng lúc.
    /// </summary>
    Active,

    /// <summary>
    /// Thẻ đã được gán dài hạn cho một vé tháng hoạt động (Monthly Subscription).
    /// Thẻ ở trạng thái này sẽ không chuyển sang Active khi xe vào bãi 
    /// và không trả về Available khi xe ra bãi.
    /// </summary>
    Assigned,

    /// <summary>
    /// Thẻ đã bị báo mất.
    /// Khi Staff chuyển thẻ sang trạng thái này, hệ thống sẽ:
    ///   1. Áp dụng phí phạt mất thẻ (lost_card_penalty) vào tổng phí của session.
    ///   2. Chỉ cho phép xe rời bãi sau khi khách thanh toán toàn bộ phí + phạt.
    /// Tham chiếu SRS: BR-052, BR-053
    /// </summary>
    Lost,

    /// <summary>
    /// Thẻ bị khoá, không được sử dụng cho bất kỳ lượt gửi xe nào.
    /// Thường áp dụng khi thẻ bị hư hỏng hoặc vi phạm nghiêm trọng.
    /// </summary>
    Blocked
}


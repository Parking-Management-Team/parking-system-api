namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái vòng đời của lượt gửi xe (ParkingSession).
/// Tham chiếu SRS: §6.13 — Parking Session Status
/// </summary>
public static class SessionStatus
{
    /// <summary>
    /// Session đang mở — xe đã check-in và vẫn còn trong bãi.
    /// Zone/capacity tương ứng đang bị giữ.
    /// Nếu check_out_time đã được ghi nhận nhưng Payment còn PENDING, session vẫn chưa COMPLETED.
    /// </summary>
    public const string Active = "ACTIVE";

    /// <summary>
    /// Session đã kết thúc sau khi check-out thành công:
    /// amount_due = 0 hoặc khoản cần thanh toán đã PAID.
    /// Zone/Slot vật lý được giải phóng sau thời điểm này.
    /// </summary>
    public const string Completed = "COMPLETED";

    /// <summary>
    /// Người gửi xe bị mất vé/mã gửi xe.
    /// Hệ thống xử lý theo luồng lost card penalty trước khi cho xe ra.
    /// </summary>
    public const string Lost = "LOST";

    /// <summary>
    /// Vé/session hết hạn hoặc không còn hợp lệ theo chính sách thời gian.
    /// </summary>
    public const string Expired = "EXPIRED";

    /// <summary>
    /// Quyền lợi thẻ tháng bị downgrade — thẻ tháng hết hạn khi xe vẫn còn trong bãi.
    /// Từ thời điểm hết hạn, hệ thống chuyển sang tính phí vãng lai.
    /// </summary>
    public const string Downgraded = "DOWNGRADED";
}

namespace PBMS.Domain.Enums;

/// <summary>
/// Trạng thái vòng đời của một lượt đặt chỗ trước (Booking).
/// Tham chiếu SRS: §6.x — Booking Status Lifecycle
/// </summary>
public static class BookingStatus
{
    /// <summary>
    /// Booking đã được tạo, đang chờ Driver thanh toán tiền cọc (Deposit).
    /// Hệ thống đã tạm giữ 1 General Capacity tại Building.
    /// Nếu quá PaymentDeadline mà chưa thanh toán → chuyển sang Expired.
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// Driver đã thanh toán tiền cọc thành công.
    /// Chỗ được giữ chắc chắn cho đến CheckinGraceUntil.
    /// Nếu quá CheckinGraceUntil mà xe chưa vào bãi → chuyển sang NoShow.
    /// </summary>
    public const string Confirmed = "Confirmed";

    /// <summary>
    /// Driver đã thực sự check-in vào bãi, Booking đã hoàn thành vai trò.
    /// Capacity được chuyển sang giữ bởi ParkingSession.
    /// </summary>
    public const string CheckedIn = "CheckedIn";

    /// <summary>
    /// Booking bị hủy (do Driver chủ động hủy hoặc do hệ thống).
    /// Capacity được giải phóng ngay khi hủy.
    /// </summary>
    public const string Cancelled = "Cancelled";

    /// <summary>
    /// Driver đã xác nhận nhưng không đến trong thời gian grace period.
    /// Capacity được giải phóng, tiền cọc không hoàn lại.
    /// </summary>
    public const string NoShow = "NoShow";

    /// <summary>
    /// Booking hết hạn do Driver không thanh toán tiền cọc trong PaymentDeadline.
    /// Capacity được giải phóng tự động.
    /// </summary>
    public const string Expired = "Expired";
}

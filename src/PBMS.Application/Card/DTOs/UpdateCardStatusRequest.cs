using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO nhận yêu cầu cập nhật trạng thái thẻ gửi xe.
///
/// Dùng cho: PATCH /api/cards/{id}/status
/// Actor: Parking Staff, Parking Manager
///
/// Tách riêng khỏi UpdateCardRequest vì đây là thao tác có tầm quan trọng
/// nghiệp vụ cao — cần validate theo state machine và ghi audit log.
///
/// Các giá trị NewStatus hợp lệ và luồng cho phép:
///   "Blocked"   (Ngưng hoạt động) → Staff/Admin khóa thẻ đang Available
///   "Available" (Mở lại)          → Admin mở lại thẻ đang Blocked hoặc Lost
///   "Lost"      (Báo mất)         → Staff báo mất thẻ đang Active trong session
/// </summary>
public class UpdateCardStatusRequest
{
    /// <summary>
    /// Trạng thái mới muốn chuyển sang.
    ///
    /// Phải là một trong các giá trị hợp lệ của enum CardStatus:
    ///   "Available", "Active", "Lost", "Blocked"
    ///
    /// Lưu ý: hệ thống sẽ validate theo state machine trước khi chấp nhận.
    /// </summary>
    [Required(ErrorMessage = "Trạng thái mới (NewStatus) không được để trống.")]
    [MaxLength(20, ErrorMessage = "Giá trị trạng thái không được vượt quá 20 ký tự.")]
    public string NewStatus { get; set; } = null!;

    /// <summary>
    /// Lý do thay đổi trạng thái (tùy chọn).
    /// Dùng cho audit log — theo dõi ai đổi trạng thái vì lý do gì.
    /// Ví dụ: "Thẻ bị hư vật lý", "Khách báo mất tại bãi B2", "Mở lại sau kiểm tra"
    /// </summary>
    [MaxLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự.")]
    public string? Reason { get; set; }
}

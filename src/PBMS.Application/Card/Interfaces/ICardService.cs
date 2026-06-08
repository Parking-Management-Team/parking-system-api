using PBMS.Application.Card.DTOs;

namespace PBMS.Application.Card.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Thẻ gửi xe (Card Management).
///
/// Các chức năng theo Acceptance Criteria:
///   Scenario 1 — CreateCardAsync      : Tạo thẻ mới
///   Scenario 2 — DeleteCardAsync      : Từ chối xóa thẻ đang trong session ACTIVE
///   Scenario 3 — GetCardByRfidAsync   : Tra cứu thông tin thẻ nhanh bằng mã RFID
///
/// Tính năng Card Status:
///   UpdateCardStatusAsync : Đổi trạng thái thẻ theo state machine nghiệp vụ
/// </summary>
public interface ICardService
{
    /// <summary>
    /// [Scenario 1] Tạo mới một thẻ gửi xe.
    ///
    /// Nghiệp vụ:
    ///   1. Kiểm tra RfidCode (nếu có) chưa tồn tại trong hệ thống (UNIQUE constraint).
    ///   2. Tạo thẻ mới với trạng thái mặc định là "Available".
    ///   3. Lưu vào database và trả về thông tin thẻ vừa tạo.
    ///
    /// Lỗi có thể xảy ra:
    ///   - RfidCode đã tồn tại → ném DomainException "RFID_CODE_EXISTS"
    /// </summary>
    Task<CardDto> CreateCardAsync(CreateCardRequest request);

    /// <summary>
    /// [Scenario 3] Tra cứu thông tin thẻ theo mã RFID.
    ///
    /// Dùng cho: Nhân viên quét thẻ hoặc nhập mã RFID để kiểm tra thông tin thẻ.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → ném NotFoundException "CARD_NOT_FOUND"
    /// </summary>
    Task<CardDto> GetCardByRfidAsync(string rfidCode);

    /// <summary>
    /// Lấy thông tin thẻ theo ID.
    /// Dùng cho: Các luồng nội bộ cần tra cứu thẻ theo ID (check-in, check-out).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → ném NotFoundException "CARD_NOT_FOUND"
    /// </summary>
    Task<CardDto> GetCardByIdAsync(int id);

    /// <summary>
    /// Cập nhật thông tin thẻ (RfidCode, BuildingId, CardType).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → NotFoundException "CARD_NOT_FOUND"
    ///   - RfidCode mới đã tồn tại → DomainException "RFID_CODE_EXISTS"
    /// </summary>
    Task<CardDto> UpdateCardAsync(int id, UpdateCardRequest request);

    /// <summary>
    /// [Scenario 2] Xóa thẻ khỏi hệ thống.
    ///
    /// Nghiệp vụ:
    ///   1. Kiểm tra thẻ có đang được gán cho ParkingSession ACTIVE không.
    ///   2. Nếu CÓ → từ chối, ném DomainException "CARD_IN_ACTIVE_SESSION".
    ///   3. Nếu KHÔNG → cho phép xóa.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → NotFoundException "CARD_NOT_FOUND"
    ///   - Thẻ đang bận → DomainException "CARD_IN_ACTIVE_SESSION"
    /// </summary>
    Task DeleteCardAsync(int id);

    /// <summary>
    /// [Card Status Feature] Cập nhật trạng thái thẻ gửi xe theo state machine nghiệp vụ.
    ///
    /// Luồng chuyển trạng thái HỢP LỆ:
    ///   Available → Blocked  : Staff/Admin khóa thẻ (Ngưng hoạt động / Inactive) — Scenario 2
    ///   Blocked   → Available: Admin mở khóa thẻ trở lại
    ///   Active    → Lost     : Staff báo mất thẻ trong khi xe đang gửi — cơ sở để Scenario 1 hoạt động
    ///   Lost      → Available: Staff/Admin xử lý xong sự cố mất thẻ
    ///
    /// Hành vi khi chuyển sang Lost:
    ///   - Hệ thống tự động set Card.LostAt = DateTime.UtcNow (ghi nhận thời điểm báo mất).
    ///   - Dùng để tính lost_card_penalty (BR-052, BR-053).
    ///
    /// Hành vi khi mở lại từ Lost → Available:
    ///   - Hệ thống reset Card.LostAt = null.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ                    → DomainException "CARD_NOT_FOUND"
    ///   - Chuyển trạng thái không hợp lệ        → DomainException "CARD_INVALID_STATUS_TRANSITION"
    ///   - Giá trị NewStatus không nhận dạng được → DomainException "CARD_STATUS_UNKNOWN"
    /// </summary>
    Task<CardDto> UpdateCardStatusAsync(int id, UpdateCardStatusRequest request);
}

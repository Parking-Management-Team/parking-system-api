using PBMS.Application.Card.DTOs;

namespace PBMS.Application.Card.Interfaces;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Thẻ gửi xe (Card Management).
///
/// Các chức năng theo Acceptance Criteria:
///   Scenario 1 — CreateCardAsync    : Tạo thẻ mới với mã chưa tồn tại
///   Scenario 2 — DeleteCardAsync    : Từ chối xóa thẻ đang trong session ACTIVE
///   Scenario 3 — GetCardByCodeAsync : Tra cứu thông tin thẻ nhanh bằng mã
/// </summary>
public interface ICardService
{
    /// <summary>
    /// [Scenario 1] Tạo mới một thẻ gửi xe.
    ///
    /// Nghiệp vụ:
    ///   1. Kiểm tra CardCode chưa tồn tại trong hệ thống (UNIQUE constraint).
    ///   2. Tạo thẻ mới với trạng thái mặc định là "Available".
    ///   3. Lưu vào database và trả về thông tin thẻ vừa tạo.
    ///
    /// Lỗi có thể xảy ra:
    ///   - CardCode đã tồn tại → ném DomainException "CARD_CODE_EXISTS"
    /// </summary>
    Task<CardDto> CreateCardAsync(CreateCardRequest request);

    /// <summary>
    /// [Scenario 3] Tra cứu thông tin thẻ theo mã định danh (CardCode).
    ///
    /// Dùng cho: Nhân viên kiểm tra thông tin thẻ bất kỳ trong bãi xe.
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → ném NotFoundException "CARD_NOT_FOUND"
    /// </summary>
    Task<CardDto> GetCardByCodeAsync(string cardCode);

    /// <summary>
    /// Lấy thông tin thẻ theo ID.
    /// Dùng cho: Các luồng nội bộ cần tra cứu thẻ theo ID (check-in, check-out).
    ///
    /// Lỗi có thể xảy ra:
    ///   - Không tìm thấy thẻ → ném NotFoundException "CARD_NOT_FOUND"
    /// </summary>
    Task<CardDto> GetCardByIdAsync(int id);

    /// <summary>
    /// Cập nhật thông tin thẻ (RfidCode, CardType).
    ///
    /// Lưu ý: CardCode KHÔNG được phép thay đổi sau khi tạo.
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
}

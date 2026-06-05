using PBMS.Domain.Entities;
// Alias rõ ràng để tránh nhầm lẫn giữa PBMS.Domain.Entities.Card (class)
// và PBMS.Application.Card (namespace/folder)
using CardEntity = PBMS.Domain.Entities.Card;

namespace PBMS.Application.Contracts;

/// <summary>
/// Hợp đồng Repository chuyên biệt cho thực thể Thẻ gửi xe (Card).
/// Định nghĩa các truy vấn nghiệp vụ riêng ngoài các hàm CRUD chung của IRepository.
///
/// Tham chiếu SRS:
///   - Acceptance Criteria Scenario 3: tra cứu thẻ nhanh bằng mã
///   - Acceptance Criteria Scenario 2: kiểm tra thẻ có đang trong session không trước khi xóa
/// </summary>
public interface ICardRepository : IRepository<CardEntity>
{
    /// <summary>
    /// Tìm thẻ theo mã định danh nội bộ (CardCode).
    /// Dùng cho: Tra cứu nhanh thẻ bằng mã (Scenario 3).
    ///
    /// Ví dụ: GetByCardCodeAsync("CARD-001")
    /// </summary>
    /// <param name="cardCode">Mã thẻ cần tìm.</param>
    /// <returns>Card nếu tìm thấy; null nếu không có thẻ nào khớp.</returns>
    Task<CardEntity?> GetByCardCodeAsync(string cardCode);

    /// <summary>
    /// Kiểm tra mã thẻ đã tồn tại trong hệ thống chưa.
    /// Dùng khi tạo thẻ mới để đảm bảo CardCode là duy nhất (Scenario 1).
    /// </summary>
    /// <param name="cardCode">Mã thẻ cần kiểm tra.</param>
    /// <returns>true nếu đã tồn tại; false nếu chưa.</returns>
    Task<bool> IsCardCodeExistsAsync(string cardCode);

    /// <summary>
    /// Kiểm tra thẻ có đang được gán cho một ParkingSession ACTIVE không.
    /// Dùng trước khi cho phép xóa thẻ (Scenario 2 — không xóa thẻ đang dùng).
    /// </summary>
    /// <param name="cardId">ID của thẻ cần kiểm tra.</param>
    /// <returns>true nếu thẻ đang bận (có session ACTIVE); false nếu rảnh.</returns>
    Task<bool> IsCardInActiveSessionAsync(int cardId);
}

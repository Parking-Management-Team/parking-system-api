using PBMS.Application.Contracts;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.ParkingSession.Services;

/// <summary>
/// Dịch vụ nghiệp vụ quản lý Lượt gửi xe (ParkingSession).
///
/// ⚠️ FILE NÀY ĐANG Ở TRẠNG THÁI STUB — chỉ implement phần guard kiểm tra
///    trạng thái thẻ (Card Status) theo yêu cầu Task con BE-2.
///    Toàn bộ nghiệp vụ check-in / check-out đầy đủ sẽ được implement riêng.
///
/// Task con BE-2: Tích hợp bước kiểm tra điều kiện trạng thái thẻ vào
///               quy trình xử lý điều kiện check-in xe.
/// </summary>
public class ParkingSessionService
{
    private readonly ICardRepository _cardRepository;

    public ParkingSessionService(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository
            ?? throw new ArgumentNullException(nameof(cardRepository));
    }

    // -----------------------------------------------------------------------
    // [CARD STATUS FEATURE — Task BE-2]
    // GUARD: KIỂM TRA ĐIỀU KIỆN TRẠNG THÁI THẺ TRƯỚC KHI CHECK-IN
    // -----------------------------------------------------------------------

    /// <summary>
    /// [Guard — Acceptance Criteria Scenario 1 &amp; 2]
    /// Kiểm tra thẻ có đủ điều kiện để thực hiện check-in hay không.
    ///
    /// Phương thức này được gọi trong luồng CheckInAsync() TRƯỚC KHI tạo ParkingSession,
    /// đảm bảo rằng không có thẻ không hợp lệ nào được tham gia vào quy trình gửi xe.
    ///
    /// Luồng xử lý:
    ///   1. Tìm thẻ theo CardCode trong hệ thống.
    ///   2. Kiểm tra trạng thái thẻ:
    ///        - Lost    → từ chối ngay, ném DomainException "CARD_IS_LOST"    (Scenario 1)
    ///        - Blocked → từ chối ngay, ném DomainException "CARD_IS_BLOCKED" (Scenario 2)
    ///        - Active  → từ chối (thẻ đang được dùng trong session khác)
    ///        - Available → cho phép tiếp tục check-in
    ///   3. Trả về Card entity nếu hợp lệ — để service tiếp tục gán vào session.
    ///
    /// Lỗi có thể xảy ra:
    ///   - CARD_NOT_FOUND              : Không tìm thấy thẻ theo CardCode.
    ///   - CARD_IS_LOST                : Thẻ đã bị báo mất — chặn tuyệt đối (Scenario 1).
    ///   - CARD_IS_BLOCKED             : Thẻ đang bị khóa (Ngưng hoạt động) (Scenario 2).
    ///   - CARD_ALREADY_IN_USE         : Thẻ đang được dùng trong session khác.
    /// </summary>
    /// <param name="cardCode">Mã thẻ cần kiểm tra khi check-in.</param>
    /// <returns>Card entity hợp lệ, sẵn sàng được gán vào ParkingSession mới.</returns>
    public async Task<Domain.Entities.Card> ValidateCardForCheckInAsync(string cardCode)
    {
        // Bước 1: Chuẩn hóa và tìm thẻ theo mã
        var normalizedCode = cardCode.Trim().ToUpper();
        var card = await _cardRepository.GetByCardCodeAsync(normalizedCode);

        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có mã '{normalizedCode}' trong hệ thống."
            );
        }

        // Bước 2: Kiểm tra trạng thái thẻ — từ chối ngay nếu không hợp lệ
        // Parse sang enum để so sánh type-safe, tránh magic string
        if (!Enum.TryParse<CardStatus>(card.CardStatus, ignoreCase: true, out var currentStatus))
        {
            throw new DomainException(
                errorCode: "CARD_STATUS_CORRUPTED",
                message: $"Trạng thái thẻ '{card.CardCode}' không hợp lệ. Liên hệ Admin."
            );
        }

        switch (currentStatus)
        {
            case CardStatus.Lost:
                // [Scenario 1] Thẻ báo mất bị từ chối khi thực hiện check-in.
                // Hệ thống lập tức chặn lại, từ chối tạo phiên gửi xe
                // và đưa ra cảnh báo thẻ đã bị báo mất.
                throw new DomainException(
                    errorCode: "CARD_IS_LOST",
                    message: $"Thẻ '{card.CardCode}' đã bị báo mất "
                           + (card.LostAt.HasValue
                               ? $"vào lúc {card.LostAt.Value:dd/MM/yyyy HH:mm} UTC. "
                               : ". ")
                           + "Không thể sử dụng thẻ này để check-in. "
                           + "Vui lòng liên hệ nhân viên để được hỗ trợ."
                );

            case CardStatus.Blocked:
                // [Scenario 2] Thẻ đang bị khóa (Ngưng hoạt động / Inactive).
                // Không thể dùng để quét xe vào được nữa.
                throw new DomainException(
                    errorCode: "CARD_IS_BLOCKED",
                    message: $"Thẻ '{card.CardCode}' đang ở trạng thái Ngưng hoạt động (Bị khóa). "
                           + "Không thể thực hiện check-in. "
                           + "Vui lòng liên hệ nhân viên để mở khóa thẻ."
                );

            case CardStatus.Active:
                // Thẻ đang được gán cho một session khác — không thể dùng đồng thời
                throw new DomainException(
                    errorCode: "CARD_ALREADY_IN_USE",
                    message: $"Thẻ '{card.CardCode}' đang được sử dụng trong một phiên gửi xe khác. "
                           + "Mỗi thẻ chỉ được dùng cho một lượt gửi xe tại một thời điểm."
                );

            case CardStatus.Available:
                // Thẻ hợp lệ — cho phép tiếp tục check-in
                return card;

            default:
                // Trường hợp enum có giá trị mới chưa được xử lý
                throw new DomainException(
                    errorCode: "CARD_STATUS_UNHANDLED",
                    message: $"Trạng thái thẻ '{currentStatus}' chưa được xử lý trong quy trình check-in."
                );
        }
    }
}

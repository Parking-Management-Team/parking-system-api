using PBMS.Application.Card.DTOs;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Card.Services;

/// <summary>
/// Triển khai nghiệp vụ quản lý Thẻ gửi xe (ICardService).
///
/// Lớp này chứa toàn bộ business logic liên quan đến Card:
///   - Tạo thẻ mới (CreateCardCommand)
///   - Tra cứu thẻ theo mã (GetCardByCodeQuery)
///   - Cập nhật thông tin thẻ
///   - Xóa thẻ (có kiểm tra session đang mở)
///
/// Nguyên tắc: Service KHÔNG biết về EF Core hay Database.
///             Chỉ giao tiếp với DB thông qua ICardRepository (Dependency Inversion).
/// </summary>
public class CardService : ICardService
{
    // Repository dùng để truy vấn và lưu dữ liệu thẻ
    private readonly ICardRepository _cardRepository;

    /// <summary>
    /// Constructor nhận ICardRepository qua Dependency Injection.
    /// ASP.NET Core DI container sẽ tự động inject khi service được khởi tạo.
    /// </summary>
    public CardService(ICardRepository cardRepository)
    {
        // Defensive check: đảm bảo repository không bị inject null
        _cardRepository = cardRepository
            ?? throw new ArgumentNullException(nameof(cardRepository));
    }

    // -----------------------------------------------------------------------
    // [Scenario 1] TẠO THẺ MỚI
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo mới một thẻ gửi xe với mã định danh chưa tồn tại trong hệ thống.
    ///
    /// Luồng xử lý:
    ///   1. Chuẩn hóa mã thẻ (trim khoảng trắng, chuyển hoa)
    ///   2. Kiểm tra mã thẻ chưa tồn tại → bảo đảm UNIQUE constraint
    ///   3. Tạo entity Card mới với trạng thái Available
    ///   4. Lưu vào DB → trả về DTO
    /// </summary>
    public async Task<CardDto> CreateCardAsync(CreateCardRequest request)
    {
        // Bước 1: Chuẩn hóa mã thẻ — loại bỏ khoảng trắng thừa
        // Mã thẻ được chuyển thành chữ HOA để tránh trùng do case khác nhau
        // Ví dụ: "card-001" và "CARD-001" được coi là giống nhau
        var normalizedCode = request.CardCode.Trim().ToUpper();

        // Bước 2: Kiểm tra mã thẻ đã tồn tại chưa
        // Nếu đã tồn tại → ném DomainException ngay, không tiếp tục
        var isExists = await _cardRepository.IsCardCodeExistsAsync(normalizedCode);
        if (isExists)
        {
            // [BR] Mã thẻ phải UNIQUE — không được tạo thẻ trùng mã
            throw new DomainException(
                errorCode: "CARD_CODE_EXISTS",
                message: $"Mã thẻ '{normalizedCode}' đã tồn tại trong hệ thống. Vui lòng dùng mã khác."
            );
        }

        // Bước 3: Tạo entity Card mới
        // Trạng thái mặc định là "Available" — thẻ sẵn sàng được dùng
        var card = new Domain.Entities.Card
        {
            CardCode = normalizedCode,
            NfcUid = request.NfcUid?.Trim(),
            CardType = request.CardType.Trim(),
            CardStatus = Domain.Enums.CardStatus.Available.ToString()
            // CreatedAt được tự động set bởi BaseEntity (DateTime.UtcNow)
        };

        // Bước 4: Lưu vào database thông qua Repository
        await _cardRepository.AddAsync(card);
        await _cardRepository.SaveChangesAsync();

        // Bước 5: Map entity vừa tạo sang DTO để trả về Client
        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // [Scenario 3] TRA CỨU THẺ THEO MÃ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tra cứu thông tin chi tiết thẻ bằng mã định danh (CardCode).
    ///
    /// Luồng xử lý:
    ///   1. Chuẩn hóa mã tìm kiếm
    ///   2. Truy vấn repository
    ///   3. Nếu không tìm thấy → ném NotFoundException
    ///   4. Map và trả về DTO
    /// </summary>
    public async Task<CardDto> GetCardByCodeAsync(string cardCode)
    {
        // Chuẩn hóa đầu vào để tìm kiếm không phân biệt hoa thường
        var normalizedCode = cardCode.Trim().ToUpper();

        // Gọi Repository tìm thẻ theo mã
        var card = await _cardRepository.GetByCardCodeAsync(normalizedCode);

        // Nếu không tìm thấy → thông báo lỗi rõ ràng
        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có mã '{normalizedCode}' trong hệ thống."
            );
        }

        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // LẤY DANH SÁCH TOÀN BỘ THẺ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách toàn bộ thẻ gửi xe trong hệ thống.
    ///
    /// Luồng xử lý:
    ///   1. Truy vấn toàn bộ danh sách thẻ từ repository
    ///   2. Map danh sách entity sang danh sách DTO
    ///   3. Trả về kết quả
    /// </summary>
    public async Task<List<CardDto>> GetAllCardsAsync()
    {
        // Gọi repository để lấy toàn bộ thực thể thẻ
        var cards = await _cardRepository.GetAllAsync();

        // Chuyển đổi danh sách entity sang DTO và trả về
        return cards.Select(MapToDto).ToList();
    }

    // -----------------------------------------------------------------------
    // LẤY THẺ THEO ID
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy thông tin thẻ theo ID.
    /// Dùng cho các luồng nội bộ (check-in, check-out cần truy vấn thẻ theo ID).
    /// </summary>
    public async Task<CardDto> GetCardByIdAsync(int id)
    {
        var card = await _cardRepository.GetByIdAsync(id);

        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có ID '{id}' trong hệ thống."
            );
        }

        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // CẬP NHẬT THẺ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thông tin thẻ (chỉ RfidCode và CardType).
    /// CardCode KHÔNG được phép thay đổi sau khi tạo (bất biến sau khi gán).
    /// </summary>
    public async Task<CardDto> UpdateCardAsync(int id, UpdateCardRequest request)
    {
        // Tìm thẻ cần cập nhật
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có ID '{id}'."
            );
        }

        // Cập nhật từng trường nếu Client có gửi giá trị mới
        // Dùng pattern "chỉ cập nhật khi không null" để hỗ trợ partial update
        if (request.NfcUid != null)
        {
            var trimmedNfcUid = request.NfcUid.Trim();

            // Kiểm tra RfidCode mới không trùng với thẻ khác
            // (bỏ qua nếu RfidCode mới trùng với RfidCode hiện tại của chính thẻ này)
            if (trimmedNfcUid != card.NfcUid)
            {
                var nfcUidExists = await _cardRepository.AnyAsync(
                    c => c.NfcUid == trimmedNfcUid && c.Id != id
                );
                if (nfcUidExists)
                {
                    throw new DomainException(
                        errorCode: "NFC_UID_EXISTS",
                        message: $"NFC UID '{trimmedNfcUid}' đã được gán cho thẻ khác."
                    );
                }
            }

            card.NfcUid = trimmedNfcUid;
        }

        if (request.CardType != null)
        {
            card.CardType = request.CardType.Trim();
        }

        // Lưu thay đổi vào database
        _cardRepository.Update(card);
        await _cardRepository.SaveChangesAsync();

        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // [Scenario 2] XÓA THẺ — KIỂM TRA TRƯỚC KHI CHO PHÉP XÓA
    // -----------------------------------------------------------------------

    /// <summary>
    /// Xóa thẻ khỏi hệ thống.
    ///
    /// Luồng xử lý theo Scenario 2:
    ///   1. Tìm thẻ cần xóa → nếu không có thì báo lỗi CARD_NOT_FOUND
    ///   2. Kiểm tra thẻ có đang bận trong ParkingSession ACTIVE không
    ///      → Nếu CÓ: từ chối xóa, ném DomainException "CARD_IN_ACTIVE_SESSION"
    ///      → Nếu KHÔNG: tiến hành xóa
    /// </summary>
    public async Task DeleteCardAsync(int id)
    {
        // Bước 1: Tìm thẻ cần xóa
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có ID '{id}'."
            );
        }

        // Bước 2: [Scenario 2] Kiểm tra thẻ có đang trong phiên gửi xe chưa hoàn thành
        // Acceptance Criteria: "Hệ thống từ chối và thông báo rõ thẻ đang bận vận hành"
        var isInActiveSession = await _cardRepository.IsCardInActiveSessionAsync(id);
        if (isInActiveSession)
        {
            throw new DomainException(
                errorCode: "CARD_IN_ACTIVE_SESSION",
                message: $"Thẻ '{card.CardCode}' đang được sử dụng trong một lượt gửi xe chưa hoàn thành. " +
                         "Vui lòng chờ lượt gửi xe kết thúc trước khi xóa thẻ."
            );
        }

        // Bước 3: Thẻ không bận → cho phép xóa
        await _cardRepository.RemoveAsync(card);
        await _cardRepository.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // HELPER — MAP ENTITY → DTO (Dùng nội bộ trong Service)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Chuyển đổi Card entity sang CardDto để trả về Client.
    ///
    /// Tách riêng thành hàm helper để tránh lặp code ở nhiều chỗ.
    /// Trong dự án lớn hơn, có thể dùng AutoMapper để làm việc này tự động.
    /// </summary>
    /// <param name="card">Entity Card từ database.</param>
    /// <returns>CardDto chứa thông tin trả về Client.</returns>
    private static CardDto MapToDto(Domain.Entities.Card card)
    {
        return new CardDto
        {
            Id = card.Id,
            CardCode = card.CardCode,
            NfcUid = card.NfcUid,
            CardType = card.CardType,
            CardStatus = card.CardStatus,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt
        };
    }
}

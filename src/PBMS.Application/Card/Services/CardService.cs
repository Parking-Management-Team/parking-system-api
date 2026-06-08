using PBMS.Application.Card.DTOs;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Domain.Exceptions;

namespace PBMS.Application.Card.Services;

/// <summary>
/// Triển khai nghiệp vụ quản lý Thẻ gửi xe (ICardService).
///
/// Lớp này chứa toàn bộ business logic liên quan đến Card:
///   - Tạo thẻ mới (CreateCardCommand)
///   - Tra cứu thẻ theo RFID (GetCardByRfidQuery)
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
    /// Tạo mới một thẻ gửi xe.
    ///
    /// Luồng xử lý:
    ///   1. Kiểm tra RfidCode (nếu có) chưa tồn tại → bảo đảm UNIQUE constraint
    ///   2. Tạo entity Card mới với trạng thái Available
    ///   3. Lưu vào DB → trả về DTO
    /// </summary>
    public async Task<CardDto> CreateCardAsync(CreateCardRequest request)
    {
        // Kiểm tra RfidCode nếu Client có gửi
        if (request.RfidCode != null)
        {
            var trimmedRfid = request.RfidCode.Trim();

            var rfidExists = await _cardRepository.IsRfidCodeExistsAsync(trimmedRfid);
            if (rfidExists)
            {
                throw new DomainException(
                    errorCode: "RFID_CODE_EXISTS",
                    message: $"Mã RFID '{trimmedRfid}' đã tồn tại trong hệ thống. Vui lòng dùng mã khác."
                );
            }
        }

        // Tạo entity Card mới
        // Trạng thái mặc định là "Available" — thẻ sẵn sàng được dùng
        var card = new Domain.Entities.Card
        {
            RfidCode   = request.RfidCode?.Trim(),
            BuildingId = request.BuildingId,
            CardType   = request.CardType.Trim(),
            CardStatus = Domain.Enums.CardStatus.Available.ToString()
            // CreatedAt được tự động set bởi BaseEntity (DateTime.UtcNow)
        };

        // Lưu vào database thông qua Repository
        await _cardRepository.AddAsync(card);
        await _cardRepository.SaveChangesAsync();

        // Map entity vừa tạo sang DTO để trả về Client
        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // [Scenario 3] TRA CỨU THẺ THEO MÃ RFID
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tra cứu thông tin chi tiết thẻ bằng mã RFID.
    ///
    /// Luồng xử lý:
    ///   1. Chuẩn hóa mã tìm kiếm (trim khoảng trắng)
    ///   2. Truy vấn repository
    ///   3. Nếu không tìm thấy → ném NotFoundException
    ///   4. Map và trả về DTO
    /// </summary>
    public async Task<CardDto> GetCardByRfidAsync(string rfidCode)
    {
        // Chuẩn hóa đầu vào để tìm kiếm nhất quán
        var normalized = rfidCode.Trim();

        // Gọi Repository tìm thẻ theo mã RFID
        var card = await _cardRepository.GetByRfidCodeAsync(normalized);

        // Nếu không tìm thấy → thông báo lỗi rõ ràng
        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có mã RFID '{normalized}' trong hệ thống."
            );
        }

        return MapToDto(card);
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
    /// Cập nhật thông tin thẻ (RfidCode, BuildingId, CardType).
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
        if (request.RfidCode != null)
        {
            var trimmedRfid = request.RfidCode.Trim();

            // Kiểm tra RfidCode mới không trùng với thẻ khác
            // (bỏ qua nếu RfidCode mới trùng với RfidCode hiện tại của chính thẻ này)
            if (trimmedRfid != card.RfidCode)
            {
                var rfidExists = await _cardRepository.AnyAsync(
                    c => c.RfidCode == trimmedRfid && c.Id != id
                );
                if (rfidExists)
                {
                    throw new DomainException(
                        errorCode: "RFID_CODE_EXISTS",
                        message: $"Mã RFID '{trimmedRfid}' đã được gán cho thẻ khác."
                    );
                }
            }

            card.RfidCode = trimmedRfid;
        }

        // BuildingId: cập nhật khi key xuất hiện trong request
        // Dùng cách gán trực tiếp (kể cả null) vì int? cần hỗ trợ xóa liên kết
        if (request.BuildingId.HasValue || request.BuildingId == null)
        {
            // Gán bất kể null hay có giá trị — cho phép xóa liên kết tòa nhà
            card.BuildingId = request.BuildingId;
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
                message: $"Thẻ ID '{id}' đang được sử dụng trong một lượt gửi xe chưa hoàn thành. " +
                         "Vui lòng chờ lượt gửi xe kết thúc trước khi xóa thẻ."
            );
        }

        // Bước 3: Thẻ không bận → cho phép xóa
        await _cardRepository.RemoveAsync(card);
        await _cardRepository.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // [CARD STATUS FEATURE] ĐỔI TRẠNG THÁI THẺ THEO STATE MACHINE
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật trạng thái thẻ gửi xe theo state machine nghiệp vụ.
    ///
    /// Task con BE-1: mở rộng xử lý kiểm tra luồng chuyển đổi trạng thái thẻ
    ///               và bổ sung ghi nhận thời điểm báo mất (LostAt).
    ///
    /// Luồng xử lý:
    ///   1. Tìm thẻ theo ID.
    ///   2. Parse NewStatus sang enum CardStatus.
    ///   3. Kiểm tra chuyển trạng thái hợp lệ theo state machine.
    ///   4. Áp dụng side effect:
    ///        Lost      → set LostAt = DateTime.UtcNow
    ///        Available (từ Lost) → reset LostAt = null
    ///   5. Lưu vào DB và trả về CardDto.
    /// </summary>
    public async Task<CardDto> UpdateCardStatusAsync(int id, UpdateCardStatusRequest request)
    {
        // Bước 1: Tìm thẻ — throw nếu không tồn tại
        var card = await _cardRepository.GetByIdAsync(id);
        if (card == null)
        {
            throw new DomainException(
                errorCode: "CARD_NOT_FOUND",
                message: $"Không tìm thấy thẻ có ID '{id}'."
            );
        }

        // Bước 2: Parse NewStatus — kiểm tra giá trị có thuộc enum không
        if (!Enum.TryParse<CardStatus>(request.NewStatus.Trim(), ignoreCase: true, out var targetStatus))
        {
            throw new DomainException(
                errorCode: "CARD_STATUS_UNKNOWN",
                message: $"Trạng thái '{request.NewStatus}' không hợp lệ. "
                       + "Các giá trị cho phép: Available, Active, Lost, Blocked."
            );
        }

        // Bước 3: Đọc trạng thái hiện tại để kiểm tra state machine
        // Dữ liệu trong DB lưu dạng string nên cần parse ra enum để so sánh an toàn
        if (!Enum.TryParse<CardStatus>(card.CardStatus, ignoreCase: true, out var currentStatus))
        {
            // Trường hợp bất thường: data trong DB bị hỏng — không nên xảy ra
            throw new DomainException(
                errorCode: "CARD_STATUS_CORRUPTED",
                message: $"Trạng thái hiện tại '{card.CardStatus}' của thẻ ID '{id}' "
                       + "không hợp lệ. Liên hệ Admin để kiểm tra dữ liệu."
            );
        }

        // Bước 4: Kiểm tra chuyển trạng thái có hợp lệ không
        if (!IsValidStatusTransition(currentStatus, targetStatus))
        {
            throw new DomainException(
                errorCode: "CARD_INVALID_STATUS_TRANSITION",
                message: $"Không thể chuyển trạng thái thẻ ID '{id}' "
                       + $"từ '{currentStatus}' sang '{targetStatus}'. "
                       + "Vui lòng kiểm tra lại luồng chuyển trạng thái hợp lệ."
            );
        }

        // Bước 5: Áp dụng side effect theo target status
        if (targetStatus == CardStatus.Lost)
        {
            // Ghi nhận thời điểm báo mất (Task con BE-1: bổ sung ghi nhận thời điểm báo mất)
            // Dùng UTC để tuyệt đối không phụ thuộc timezone của server
            card.LostAt = DateTime.UtcNow;
        }
        else if (targetStatus == CardStatus.Available && currentStatus == CardStatus.Lost)
        {
            // Mở lại thẻ từ trạng thái Lost — reset LostAt về null
            card.LostAt = null;
        }

        // Bước 6: Cập nhật trạng thái mới vào entity
        card.CardStatus = targetStatus.ToString();

        // Bước 7: Lưu vào database thông qua Repository
        _cardRepository.Update(card);
        await _cardRepository.SaveChangesAsync();

        return MapToDto(card);
    }

    // -----------------------------------------------------------------------
    // STATE MACHINE HELPER — Kiểm tra chuyển trạng thái hợp lệ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Kiểm tra liệu việc chuyển từ <paramref name="current"/> sang
    /// <paramref name="target"/> có được phép theo nghiệp vụ không.
    ///
    /// Bảng chuyển trạng thái HOẠT ĐỘNG:
    ///   Available → Blocked  : Staff/Admin khóa thẻ (Ngưng hoạt động)
    ///   Blocked   → Available: Admin mở khóa trở lại
    ///   Active    → Lost     : Staff báo mất trong session đang mở
    ///   Lost      → Available: Staff/Admin giải quyết xong sự cố
    /// </summary>
    private static bool IsValidStatusTransition(CardStatus current, CardStatus target)
    {
        return (current, target) switch
        {
            // Staff/Admin khóa thẻ đang rảnh (Ngưng hoạt động / Inactive)
            (CardStatus.Available, CardStatus.Blocked)   => true,
            // Admin mở lại thẻ đã bị khóa
            (CardStatus.Blocked,   CardStatus.Available) => true,
            // Staff báo mất thẻ trong khi xe đang gửi (phải đang Active)
            (CardStatus.Active,    CardStatus.Lost)      => true,
            // Staff/Admin giải quyết xong sự cố mất thẻ
            (CardStatus.Lost,      CardStatus.Available) => true,
            // Tất cả các chuyển đổi khác đều bị từ chối
            _                                            => false
        };
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
            Id         = card.Id,
            RfidCode   = card.RfidCode,
            BuildingId = card.BuildingId,
            CardType   = card.CardType,
            CardStatus = card.CardStatus,
            LostAt     = card.LostAt,
            CreatedAt  = card.CreatedAt
        };
    }
}

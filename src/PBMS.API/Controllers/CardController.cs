using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Card.DTOs;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Common;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller quản lý Thẻ gửi xe (Card Management).
/// Endpoint chính: /api/cards
///
/// Actor: Parking Staff (nhân viên bãi xe)
///
/// Các chức năng:
///   POST   /api/cards                     → Tạo thẻ mới (Scenario 1)
///   GET    /api/cards/{id}                → Lấy thông tin thẻ theo ID
///   GET    /api/cards/by-code/{cardCode}  → Tra cứu thẻ theo mã (Scenario 3)
///   PUT    /api/cards/{id}                → Cập nhật thông tin thẻ
///   DELETE /api/cards/{id}                → Xóa thẻ (Scenario 2 — từ chối nếu đang bận)
///   GET   /api/cards                      → Lấy danh sách thẻ (áp dụng bộ lọc)
/// </summary>
[ApiController]
[Route("api/cards")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;

    /// <summary>
    /// Constructor nhận ICardService qua Dependency Injection.
    /// </summary>
    public CardController(ICardService cardService)
    {
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
    }

    // -----------------------------------------------------------------------
    // POST /api/cards — [Scenario 1] Tạo thẻ mới
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo mới một thẻ gửi xe với mã định danh chưa tồn tại.
    ///
    /// Route  : POST /api/cards
    /// Body   : CreateCardRequest (CardCode bắt buộc, RfidCode và CardType tùy chọn)
    /// Returns: 201 Created + CardDto nếu thành công
    ///          409 Conflict nếu CardCode đã tồn tại
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<CardDto>>> CreateCard([FromBody] CreateCardRequest request)
    {
        // Gọi service xử lý nghiệp vụ tạo thẻ
        // (Service sẽ tự kiểm tra UNIQUE CardCode và ném DomainException nếu trùng)
        var card = await _cardService.CreateCardAsync(request);

        // 201 Created: trả về URL của resource mới tạo + data
        return CreatedAtAction(
            actionName: nameof(GetCardById),       // Trỏ tới endpoint GET /api/cards/{id}
            routeValues: new { id = card.Id },     // Giá trị route parameter
            value: BaseResponse<CardDto>.Ok(card, "Tạo thẻ gửi xe thành công.")
        );
    }

        // -----------------------------------------------------------------------
    // GET /api/cards/ — Lấy danh sách thẻ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách toàn bộ thẻ gửi xe.
    ///
    /// Route  : GET /api/cards
    /// Returns: 200 OK + Danh sách CardDto (danh sách rỗng nếu chưa có thẻ nào)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<CardDto>>>> GetAllCards()
    {
        var card = await _cardService.GetAllCardsAsync();

        return Ok(BaseResponse<List<CardDto>>.Ok(card));
    }

    // -----------------------------------------------------------------------
    // GET /api/cards/{id} — Lấy thông tin thẻ theo ID
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy thông tin chi tiết thẻ theo ID.
    ///
    /// Route  : GET /api/cards/{id}
    /// Returns: 200 OK + CardDto nếu tìm thấy
    ///          404 Not Found nếu không có thẻ với ID này
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<CardDto>>> GetCardById(int id)
    {
        var card = await _cardService.GetCardByIdAsync(id);

        return Ok(BaseResponse<CardDto>.Ok(card));
    }

    // -----------------------------------------------------------------------
    // GET /api/cards/by-code/{cardCode} — [Scenario 3] Tra cứu thẻ theo mã
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tra cứu thông tin thẻ nhanh bằng mã định danh (CardCode).
    ///
    /// Đây là endpoint chính cho Scenario 3:
    ///   "Nhân viên nhập mã thẻ vào ô tìm kiếm
    ///    → Hệ thống trả về thông tin chi tiết bao gồm mã và trạng thái hiện thời"
    ///
    /// Route  : GET /api/cards/by-code/{cardCode}
    ///          Ví dụ: GET /api/cards/by-code/CARD-001
    /// Returns: 200 OK + CardDto nếu tìm thấy
    ///          404 Not Found nếu không có thẻ với mã này
    /// </summary>
    [HttpGet("by-code/{cardCode}")]
    public async Task<ActionResult<BaseResponse<CardDto>>> GetCardByCode(string cardCode)
    {
        var card = await _cardService.GetCardByCodeAsync(cardCode);

        return Ok(BaseResponse<CardDto>.Ok(card));
    }

    // -----------------------------------------------------------------------
    // PUT /api/cards/{id} — Cập nhật thông tin thẻ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thông tin thẻ (RfidCode và CardType).
    ///
    /// Lưu ý: CardCode KHÔNG thể thay đổi sau khi tạo.
    ///
    /// Route  : PUT /api/cards/{id}
    /// Body   : UpdateCardRequest
    /// Returns: 200 OK + CardDto sau khi cập nhật
    ///          404 Not Found nếu không có thẻ
    ///          409 Conflict nếu RfidCode mới đã trùng với thẻ khác
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<CardDto>>> UpdateCard(
        int id,
        [FromBody] UpdateCardRequest request)
    {
        var card = await _cardService.UpdateCardAsync(id, request);

        return Ok(BaseResponse<CardDto>.Ok(card, "Cập nhật thông tin thẻ thành công."));
    }

    // -----------------------------------------------------------------------
    // DELETE /api/cards/{id} — [Scenario 2] Xóa thẻ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Xóa thẻ khỏi hệ thống.
    ///
    /// Theo Scenario 2: Nếu thẻ đang được gán cho ParkingSession chưa hoàn thành,
    /// hệ thống sẽ TỪ CHỐI và trả về 409 Conflict với thông báo rõ ràng.
    ///
    /// Route  : DELETE /api/cards/{id}
    /// Returns: 204 No Content nếu xóa thành công
    ///          404 Not Found nếu không tìm thấy thẻ
    ///          409 Conflict nếu thẻ đang trong session ACTIVE
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteCard(int id)
    {
        await _cardService.DeleteCardAsync(id);

        // 204 No Content: xóa thành công, không có data trả về
        return NoContent();
    }
}

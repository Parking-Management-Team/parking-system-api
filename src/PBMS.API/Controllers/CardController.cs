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
///   POST   /api/cards                        → Tạo thẻ mới (Scenario 1)
///   GET    /api/cards/{id}                   → Lấy thông tin thẻ theo ID
///   GET    /api/cards/by-rfid/{rfidCode}     → Tra cứu thẻ theo mã RFID (Scenario 3)
///   PUT    /api/cards/{id}                   → Cập nhật thông tin thẻ (RfidCode, BuildingId, CardType)
///   PATCH  /api/cards/{id}/status            → Cập nhật trạng thái thẻ (Card Status Feature)
///   DELETE /api/cards/{id}                   → Xóa thẻ (Scenario 2 — từ chối nếu đang bận)
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
    /// Tạo mới một thẻ gửi xe.
    ///
    /// Route  : POST /api/cards
    /// Body   : CreateCardRequest (RfidCode và BuildingId tùy chọn, CardType mặc định "PARKING_CARD")
    /// Returns: 201 Created + CardDto nếu thành công
    ///          409 Conflict nếu RfidCode đã tồn tại
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<CardDto>>> CreateCard([FromBody] CreateCardRequest request)
    {
        // Gọi service xử lý nghiệp vụ tạo thẻ
        // (Service sẽ tự kiểm tra UNIQUE RfidCode và ném DomainException nếu trùng)
        var card = await _cardService.CreateCardAsync(request);

        // 201 Created: trả về URL của resource mới tạo + data
        return CreatedAtAction(
            actionName: nameof(GetCardById),       // Trỏ tới endpoint GET /api/cards/{id}
            routeValues: new { id = card.Id },     // Giá trị route parameter
            value: BaseResponse<CardDto>.Ok(card, "Tạo thẻ gửi xe thành công.")
        );
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
    // GET /api/cards/by-rfid/{rfidCode} — [Scenario 3] Tra cứu thẻ theo RFID
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tra cứu thông tin thẻ nhanh bằng mã RFID của chip trên thẻ vật lý.
    ///
    /// Đây là endpoint chính cho Scenario 3:
    ///   "Nhân viên quét thẻ hoặc nhập mã RFID
    ///    → Hệ thống trả về thông tin chi tiết bao gồm mã và trạng thái hiện thời"
    ///
    /// Route  : GET /api/cards/by-rfid/{rfidCode}
    ///          Ví dụ: GET /api/cards/by-rfid/A3F9B2C1
    /// Returns: 200 OK + CardDto nếu tìm thấy
    ///          404 Not Found nếu không có thẻ với mã RFID này
    /// </summary>
    [HttpGet("by-rfid/{rfidCode}")]
    public async Task<ActionResult<BaseResponse<CardDto>>> GetCardByRfid(string rfidCode)
    {
        var card = await _cardService.GetCardByRfidAsync(rfidCode);

        return Ok(BaseResponse<CardDto>.Ok(card));
    }

    // -----------------------------------------------------------------------
    // PUT /api/cards/{id} — Cập nhật thông tin thẻ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thông tin thẻ (RfidCode, BuildingId và CardType).
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

    // -----------------------------------------------------------------------
    // PATCH /api/cards/{id}/status — [Card Status Feature] Đổi trạng thái thẻ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật trạng thái thẻ gửi xe theo state machine nghiệp vụ.
    ///
    /// Actor: Parking Staff, Parking Manager
    ///
    /// Dùng PATCH (không phải PUT) vì đây là partial update — chỉ thay đổi
    /// một trường duy nhất (CardStatus) chứ không cập nhật toàn bộ resource.
    ///
    /// Luồng chuyển trạng thái hợp lệ:
    ///   Available → Blocked  : Staff khóa thẻ (Ngưng hoạt động / Inactive)
    ///   Blocked   → Available: Admin mở khóa thẻ
    ///   Active    → Lost     : Staff báo mất thẻ trong session đang mở
    ///   Lost      → Available: Staff/Admin xử lý xong sự cố mất thẻ
    ///
    /// Route  : PATCH /api/cards/{id}/status
    /// Body   : UpdateCardStatusRequest { NewStatus, Reason? }
    /// Returns: 200 OK + CardDto với trạng thái mới nếu thành công
    ///          404 Not Found  nếu không tìm thấy thẻ
    ///          422 Unprocessable nếu chuyển trạng thái không hợp lệ
    ///
    /// Ví dụ body request:
    ///   { "newStatus": "Blocked", "reason": "Thẻ bị hư vật lý" }
    ///   { "newStatus": "Lost" }
    ///   { "newStatus": "Available", "reason": "Đã tìm lại thẻ" }
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<BaseResponse<CardDto>>> UpdateCardStatus(
        int id,
        [FromBody] UpdateCardStatusRequest request)
    {
        // Gọi service xử lý nghiệp vụ đổi trạng thái
        // Service sẽ validate state machine và ném DomainException nếu vi phạm
        var card = await _cardService.UpdateCardStatusAsync(id, request);

        return Ok(BaseResponse<CardDto>.Ok(card, $"Cập nhật trạng thái thẻ thành '{card.CardStatus}' thành công."));
    }
}

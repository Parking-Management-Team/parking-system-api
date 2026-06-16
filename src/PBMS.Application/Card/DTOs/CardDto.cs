namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO trả về thông tin thẻ gửi xe cho Client (Response Object).
///
/// Dùng cho tất cả các endpoint trả về thông tin Card:
///   - GET /api/cards/{id}
///   - GET /api/cards/by-code/{cardCode}
///   - POST /api/cards (sau khi tạo thành công)
///   - PUT /api/cards/{id} (sau khi cập nhật)
/// </summary>
public class CardDto
{
    /// <summary>ID định danh duy nhất của thẻ trong hệ thống.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã thẻ nội bộ — mã dùng để in ra vé/QR và tra cứu nhanh.
    /// Ví dụ: "CARD-001"
    /// </summary>
    public string CardCode { get; set; } = null!;

    /// <summary>Mã RFID mô phỏng nếu có. Null nếu thẻ không dùng RFID.</summary>
    public string? NfcUid { get; set; }

    /// <summary>Loại thẻ. Ví dụ: "PARKING_CARD"</summary>
    public string CardType { get; set; } = null!;

    /// <summary>
    /// Trạng thái hiện tại của thẻ.
    /// Các giá trị có thể: "Available", "Active", "Lost", "Blocked"
    /// </summary>
    public string CardStatus { get; set; } = null!;

    /// <summary>Thời điểm thẻ được tạo vào hệ thống.</summary>
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

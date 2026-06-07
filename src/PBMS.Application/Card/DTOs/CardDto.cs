namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO trả về thông tin thẻ gửi xe cho Client (Response Object).
///
/// Dùng cho tất cả các endpoint trả về thông tin Card:
///   - GET /api/cards/{id}
///   - GET /api/cards/by-code/{cardCode}
///   - POST /api/cards (sau khi tạo thành công)
///   - PUT /api/cards/{id} (sau khi cập nhật)
///   - PATCH /api/cards/{id}/status (sau khi đổi trạng thái)
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
    public string? RfidCode { get; set; }

    /// <summary>Loại thẻ. Ví dụ: "PARKING_CARD"</summary>
    public string CardType { get; set; } = null!;

    /// <summary>
    /// Trạng thái hiện tại của thẻ.
    /// Các giá trị có thể: "Available", "Active", "Lost", "Blocked"
    /// Lưu ý: "Blocked" tương đương "Inactive" trong giao diện người dùng.
    /// </summary>
    public string CardStatus { get; set; } = null!;

    /// <summary>
    /// Thời điểm thẻ được báo mất (Lost).
    /// Null nếu thẻ chưa từng bị báo mất hoặc đã được mở lại sau sự cố.
    /// </summary>
    public DateTime? LostAt { get; set; }

    /// <summary>Thời điểm thẻ được tạo vào hệ thống.</summary>
    public DateTime CreatedAt { get; set; }
}

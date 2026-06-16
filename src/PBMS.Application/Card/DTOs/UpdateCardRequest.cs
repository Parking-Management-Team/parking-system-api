using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO nhận dữ liệu khi cập nhật thông tin thẻ gửi xe.
///
/// Dùng cho: PUT /api/cards/{id}
/// Actor: Parking Staff hoặc Manager
///
/// Lưu ý thiết kế:
///   - Không cho phép đổi CardCode (mã định danh không được thay đổi sau khi tạo)
///   - Chỉ cho phép cập nhật RfidCode và CardType
///   - CardStatus được cập nhật riêng qua endpoint chuyên biệt (PATCH /api/cards/{id}/status)
/// </summary>
public class UpdateCardRequest
{
    /// <summary>
    /// Mã RFID mới muốn gán cho thẻ (có thể để null để xóa RFID cũ).
    /// Tối đa 50 ký tự.
    /// </summary>
    [MaxLength(50, ErrorMessage = "Mã RFID không được vượt quá 50 ký tự.")]
    public string? NfcUid { get; set; }

    /// <summary>
    /// Loại thẻ mới. Tối đa 20 ký tự.
    /// </summary>
    [MaxLength(20, ErrorMessage = "Loại thẻ không được vượt quá 20 ký tự.")]
    public string? CardType { get; set; }
}

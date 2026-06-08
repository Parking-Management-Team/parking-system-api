using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO nhận dữ liệu từ Client khi tạo mới một thẻ gửi xe.
///
/// Dùng cho: POST /api/cards
/// Actor: Parking Staff (nhân viên nhập thẻ mới vào hệ thống)
///
/// Acceptance Criteria - Scenario 1:
///   "Nhân viên nhập vào hệ thống mã RFID của thẻ mới chưa từng tồn tại
///    → Thẻ được ghi nhận thành công với trạng thái Available"
/// </summary>
public class CreateCardRequest
{
    /// <summary>
    /// Mã RFID của con chip trên thẻ vật lý — định danh duy nhất của thẻ.
    /// Không bắt buộc nếu thẻ chưa được gán chip RFID.
    /// Tối đa 50 ký tự (theo SRS §8.3.3 varchar(50))
    /// </summary>
    [MaxLength(50, ErrorMessage = "Mã RFID không được vượt quá 50 ký tự.")]
    public string? RfidCode { get; set; }

    /// <summary>
    /// ID của tòa nhà mà thẻ này thuộc về. Tùy chọn.
    /// </summary>
    public int? BuildingId { get; set; }

    /// <summary>
    /// Loại thẻ — phân biệt nhóm thẻ trong hệ thống.
    /// Mặc định: "PARKING_CARD"
    /// Tối đa 20 ký tự (theo SRS §8.3.3 varchar(20))
    /// </summary>
    [MaxLength(20, ErrorMessage = "Loại thẻ không được vượt quá 20 ký tự.")]
    public string CardType { get; set; } = "PARKING_CARD";
}

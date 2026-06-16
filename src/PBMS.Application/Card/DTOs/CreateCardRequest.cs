using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO nhận dữ liệu từ Client khi tạo mới một thẻ gửi xe.
///
/// Dùng cho: POST /api/cards
/// Actor: Parking Staff (nhân viên nhập thẻ mới vào hệ thống)
///
/// Acceptance Criteria - Scenario 1:
///   "Nhân viên nhập vào hệ thống một mã định danh thẻ mới chưa từng tồn tại
///    → Thẻ được ghi nhận thành công với trạng thái Available"
/// </summary>
public class CreateCardRequest
{
    /// <summary>
    /// Mã thẻ nội bộ — in ra vé/QR cho khách cầm khi gửi xe.
    /// Bắt buộc nhập và phải DUY NHẤT trong hệ thống.
    ///
    /// Ví dụ: "CARD-001", "PKG-2026-001"
    /// Tối đa 20 ký tự (theo SRS §8.3.3 varchar(20))
    /// </summary>
    [Required(ErrorMessage = "Mã thẻ (CardCode) là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã thẻ không được vượt quá 20 ký tự.")]
    public string CardCode { get; set; } = null!;

    /// <summary>
    /// Mã RFID mô phỏng — không bắt buộc.
    /// Chỉ dùng khi muốn mô phỏng kịch bản quét thẻ RFID.
    /// Tối đa 50 ký tự (theo SRS §8.3.3 varchar(50))
    /// </summary>
    [MaxLength(50, ErrorMessage = "Mã RFID không được vượt quá 50 ký tự.")]
    public string? NfcUid { get; set; }

    /// <summary>
    /// Loại thẻ — phân biệt nhóm thẻ trong hệ thống.
    /// Mặc định: "PARKING_CARD"
    /// Tối đa 20 ký tự (theo SRS §8.3.3 varchar(20))
    /// </summary>
    [MaxLength(20, ErrorMessage = "Loại thẻ không được vượt quá 20 ký tự.")]
    public string CardType { get; set; } = "PARKING_CARD";
}

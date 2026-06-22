using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Card.DTOs;

/// <summary>
/// DTO nhận yêu cầu cập nhật trạng thái thẻ gửi xe.
/// </summary>
public class UpdateCardStatusRequest
{
    [Required(ErrorMessage = "Trạng thái thẻ (status) là bắt buộc.")]
    public string Status { get; set; } = null!;
}

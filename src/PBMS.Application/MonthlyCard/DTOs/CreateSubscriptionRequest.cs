using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.MonthlyCard.DTOs;

/// <summary>
/// Yêu cầu đăng ký vé tháng mới.
/// </summary>
public class CreateSubscriptionRequest
{
    [Required(ErrorMessage = "AccountId là bắt buộc.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "VehicleId là bắt buộc.")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "BuildingId là bắt buộc.")]
    public int BuildingId { get; set; }

    /// <summary>
    /// ID của thẻ MONTHLY sẽ gán cho đăng ký này.
    /// Có thể cung cấp lúc đăng ký hoặc gán bổ sung lúc kích hoạt.
    /// </summary>
    public int? AssignedCardId { get; set; }
}

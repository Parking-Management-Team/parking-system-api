using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Blacklist.DTOs;

/// <summary>
/// Yêu cầu thêm một mục vào danh sách đen.
/// Hỗ trợ 2 cách: Nhập ID trực tiếp HOẶC nhập thủ công (licensePlate/cardCode) để hệ thống tự tìm ID.
/// </summary>
public class AddToBlacklistRequest : IValidatableObject
{
    public int? VehicleId { get; set; }
    public int? CardId { get; set; }
    public int? IncidentId { get; set; }

    [MaxLength(20, ErrorMessage = "License plate cannot exceed 20 characters.")]
    public string? LicensePlate { get; set; }

    [MaxLength(20, ErrorMessage = "Card code cannot exceed 20 characters.")]
    public string? CardCode { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(100, ErrorMessage = "Reason cannot exceed 100 characters.")]
    public string Reason { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool hasVehicle = VehicleId.HasValue || !string.IsNullOrWhiteSpace(LicensePlate);
        bool hasCard = CardId.HasValue || !string.IsNullOrWhiteSpace(CardCode);

        if (!hasVehicle && !hasCard && !IncidentId.HasValue)
        {
            yield return new ValidationResult(
                "You must provide at least a Vehicle (VehicleId or LicensePlate), Card (CardId or CardCode), or IncidentId to blacklist.",
                new[] { nameof(VehicleId), nameof(CardId), nameof(IncidentId), nameof(LicensePlate), nameof(CardCode) }
            );
        }
    }
}

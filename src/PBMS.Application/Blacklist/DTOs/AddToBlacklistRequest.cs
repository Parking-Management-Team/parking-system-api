using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Blacklist.DTOs;

/// <summary>
/// Yêu cầu thêm một mục vào danh sách đen.
/// Phải có ít nhất một trong ba trường: VehicleId, CardId hoặc IncidentId.
/// </summary>
public class AddToBlacklistRequest : IValidatableObject
{
    public int? VehicleId { get; set; }
    public int? CardId { get; set; }
    public int? IncidentId { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [MaxLength(100, ErrorMessage = "Reason cannot exceed 100 characters.")]
    public string Reason { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!VehicleId.HasValue && !CardId.HasValue && !IncidentId.HasValue)
        {
            yield return new ValidationResult(
                "You must provide at least a VehicleId, CardId, or IncidentId to blacklist.",
                new[] { nameof(VehicleId), nameof(CardId), nameof(IncidentId) }
            );
        }
    }
}

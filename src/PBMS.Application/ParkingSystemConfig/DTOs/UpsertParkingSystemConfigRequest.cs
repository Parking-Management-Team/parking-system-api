using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSystemConfig.DTOs;

/// <summary>
/// Request body for creating or updating a system configuration entry (upsert).
/// </summary>
public class UpsertParkingSystemConfigRequest
{
    /// <summary>
    /// Configuration key. Must be unique.
    /// Example: "BUFFER_TIME_MINUTES"
    /// </summary>
    [Required(ErrorMessage = "Key is required.")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// The new value for this configuration key.
    /// </summary>
    [Required(ErrorMessage = "Value is required.")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Optional human-readable description of what this config key controls.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Identifier of the user performing the update.
    /// </summary>
    public string? UpdatedBy { get; set; }
}

namespace PBMS.Application.ParkingSystemConfig.DTOs;

/// <summary>
/// DTO for returning a single system configuration entry.
/// </summary>
public class ParkingSystemConfigDto
{
    /// <summary>Configuration key.</summary>
    public string Key { get; set; } = null!;

    /// <summary>Configuration value.</summary>
    public string Value { get; set; } = null!;

    /// <summary>Human-readable description of the config key.</summary>
    public string? Description { get; set; }

    /// <summary>Last updated timestamp (UTC).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Identifier of the user who last updated this entry.</summary>
    public string? UpdatedBy { get; set; }
}

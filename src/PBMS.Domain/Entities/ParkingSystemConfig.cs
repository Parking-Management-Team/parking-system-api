namespace PBMS.Domain.Entities;

/// <summary>
/// Entity for storing system-wide configuration settings as key-value pairs.
/// This entity does NOT inherit BaseEntity because it uses a string primary key.
/// Examples: BUFFER_TIME_MINUTES, WALKIN_STAY_THRESHOLD_HOURS.
/// </summary>
public class ParkingSystemConfig
{
    /// <summary>
    /// Unique configuration key (primary key).
    /// Example: "BUFFER_TIME_MINUTES"
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// The configuration value stored as a string.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Optional human-readable description of what this config key controls.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp of the last update (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user who last updated this config entry.
    /// </summary>
    public string? UpdatedBy { get; set; }
}

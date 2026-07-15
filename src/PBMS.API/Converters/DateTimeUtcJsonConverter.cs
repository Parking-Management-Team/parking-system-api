using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PBMS.API.Converters
{
    /// <summary>
    /// Custom JSON converter that deserializes all incoming DateTime values into UTC
    /// and serializes all outgoing DateTime values into Vietnam local timezone (UTC+7).
    /// </summary>
    public class DateTimeUtcJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rawValue = reader.GetString();

            if (rawValue == null)
            {
                return DateTime.UtcNow;
            }

            // Date-only format (e.g., "2026-06-25"): no colon in time portion
            bool hasTime = rawValue.Contains(":");
            if (!hasTime)
            {
                // Treat as UTC midnight
                if (DateTime.TryParse(rawValue, out var dateOnly))
                    return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
                return DateTime.UtcNow;
            }

            // Try parsing as DateTimeOffset — handles +07:00, -05:00, Z, etc.
            if (DateTimeOffset.TryParse(rawValue, null, System.Globalization.DateTimeStyles.None, out var dto))
            {
                return dto.UtcDateTime;
            }

            // Fallback: try as a bare datetime with no timezone — assume Vietnam Local (+7) → convert to UTC
            if (DateTime.TryParse(rawValue, out var bare))
            {
                return DateTime.SpecifyKind(bare.AddHours(-7), DateTimeKind.Utc);
            }

            // Last resort: return UTC now
            return DateTime.UtcNow;
        }

        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert UTC value to Vietnam local time before writing with +07:00 offset
            var localTime = value.Kind == DateTimeKind.Utc 
                ? TimeZoneInfo.ConvertTimeFromUtc(value, VietnamTimeZone) 
                : value;
            writer.WriteStringValue(localTime.ToString("yyyy-MM-ddTHH:mm:ss+07:00"));
        }
    }
}

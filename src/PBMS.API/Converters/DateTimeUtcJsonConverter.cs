using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PBMS.API.Converters
{
    /// <summary>
    /// Custom JSON converter that deserializes all incoming DateTime values into UTC.
    /// Handles both inputs with timezone offsets (Z, +07:00, etc.) and unspecified local times (defaulting to Vietnam ICT +7).
    /// </summary>
    public class DateTimeUtcJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetDateTime();
            var rawValue = reader.GetString();

            if (rawValue != null)
            {
                // Check if string contains a time separator (e.g., ':')
                bool hasTime = rawValue.Contains(":");

                if (hasTime)
                {
                    // Check if it has timezone indicator (Z, +, or a offset hyphen after the time separator)
                    bool hasTimezone = rawValue.EndsWith("Z", StringComparison.OrdinalIgnoreCase) || 
                                      rawValue.Contains("+");

                    if (!hasTimezone)
                    {
                        int tIndex = rawValue.IndexOf('T');
                        if (tIndex == -1) tIndex = rawValue.IndexOf(' ');
                        
                        if (tIndex != -1)
                        {
                            int lastHyphen = rawValue.LastIndexOf('-');
                            if (lastHyphen > tIndex)
                            {
                                hasTimezone = true;
                            }
                        }
                    }

                    if (!hasTimezone)
                    {
                        // No timezone info: assume Vietnam Local time (+7) and convert to UTC
                        return DateTime.SpecifyKind(dateTime.AddHours(-7), DateTimeKind.Utc);
                    }
                }
                else
                {
                    // Date-only format (e.g., "2026-06-25"): Treat as UTC midnight directly
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }
            }

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}

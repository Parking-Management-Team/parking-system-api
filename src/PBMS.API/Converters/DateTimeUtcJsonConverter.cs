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
            var dateTime = reader.GetDateTime();
            var rawValue = reader.GetString();

            if (rawValue != null)
            {
                bool hasTime = rawValue.Contains(":");

                if (hasTime)
                {
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
            // value is stored in Vietnam local time (UTC+7), so we write it directly with +07:00 offset
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss+07:00"));
        }
    }
}

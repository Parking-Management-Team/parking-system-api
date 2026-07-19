using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PBMS.API.Converters
{
    /// <summary>
    /// Custom JSON converter that deserializes all incoming TimeSpan values.
    /// Handles various string formats such as "hh:mm", "hh:mm:ss", and standard TimeSpan format.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return TimeSpan.Zero;
            }

            // Try standard parsing first
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            // Try parsing hh:mm format explicitly (e.g. "08:00")
            if (TimeSpan.TryParseExact(value, new[] { "g", "c", "hh\\:mm", "h\\:mm", "hh\\:mm\\:ss" }, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new JsonException($"Unable to parse \"{value}\" as TimeSpan.");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("hh\\:mm\\:ss"));
        }
    }
}

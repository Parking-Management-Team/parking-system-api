using System.Text.RegularExpressions;

namespace PBMS.Application.Vehicle.Validation;

/// <summary>
/// Shared rules for normalizing, validating, formatting, and classifying
/// Vietnamese license plates.
/// </summary>
public static partial class LicensePlateValidation
{
    private static readonly HashSet<string> SpecialCarPrefixes =
    [
        "LD", "DA", "MK", "HC", "NG", "QT", "NN", "KT"
    ];

    public static string Normalize(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return string.Empty;
        }

        return new string(licensePlate
            .Trim()
            .ToUpperInvariant()
            .Where(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9')
            .ToArray());
    }

    public static bool IsValid(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return false;
        }

        var raw = licensePlate.Trim();
        if (raw.Length > 20 || raw.Any(c =>
                !(c is >= 'A' and <= 'Z'
                    or >= 'a' and <= 'z'
                    or >= '0' and <= '9'
                    or '-'
                    or '.')
                && !char.IsWhiteSpace(c)))
        {
            return false;
        }

        return NormalizedPlatePattern().IsMatch(Normalize(raw));
    }

    public static string Format(string? licensePlate)
    {
        var normalized = Normalize(licensePlate);
        var match = PlatePartsPattern().Match(normalized);
        if (!match.Success)
        {
            return normalized;
        }

        var prefix = match.Groups[1].Value;
        var suffix = match.Groups[2].Value;
        return suffix.Length == 5
            ? $"{prefix}-{suffix[..3]}.{suffix[3..]}"
            : $"{prefix}-{suffix}";
    }

    public static string DetectVehicleType(string? licensePlate)
    {
        var normalized = Normalize(licensePlate);
        var match = PlatePartsPattern().Match(normalized);
        if (!match.Success)
        {
            return "Car";
        }

        var prefix = match.Groups[1].Value;
        if (MotorcycleSeriesPattern().IsMatch(prefix))
        {
            return "Motorcycle";
        }

        if (TwoLetterSeriesPattern().IsMatch(prefix))
        {
            var letters = prefix[2..];
            return SpecialCarPrefixes.Contains(letters) ? "Car" : "Motorcycle";
        }

        return "Car";
    }

    [GeneratedRegex(@"^\d{2}(?:[A-Z]|[A-Z]\d|[A-Z]{2})\d{4,5}$",
        RegexOptions.CultureInvariant)]
    private static partial Regex NormalizedPlatePattern();

    [GeneratedRegex(@"^(.*?)(\d{4,5})$", RegexOptions.CultureInvariant)]
    private static partial Regex PlatePartsPattern();

    [GeneratedRegex(@"^\d{2}[A-Z]\d$", RegexOptions.CultureInvariant)]
    private static partial Regex MotorcycleSeriesPattern();

    [GeneratedRegex(@"^\d{2}[A-Z]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex TwoLetterSeriesPattern();
}

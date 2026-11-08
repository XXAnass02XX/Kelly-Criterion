using System.Globalization;

namespace KellyCriterion.App.Input;

public static class NumberParser
{
    public static bool TryParseInt(string? text, out int value) =>
        int.TryParse(Normalize(text), NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

    public static bool TryParseDouble(string? text, out double value) =>
        double.TryParse(Normalize(text).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        && double.IsFinite(value);

    private static string Normalize(string? text) => (text ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
}

using System.Globalization;

namespace KellyCriterion.App.Formatting;

public static class NumberFormatter
{
    public static string Compact(double value)
    {
        if (double.IsNaN(value))
            return "—";
        if (double.IsInfinity(value))
            return value > 0 ? "∞" : "-∞";
        if (value == 0)
            return "0";

        var magnitude = Math.Abs(value);
        var format = magnitude is >= 1e6 or < 1e-3 ? "0.##E+0" : "#,0.##";
        return value.ToString(format, CultureInfo.CurrentCulture);
    }
}

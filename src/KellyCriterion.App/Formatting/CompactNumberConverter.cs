using System.Globalization;
using System.Windows.Data;

namespace KellyCriterion.App.Formatting;

[ValueConversion(typeof(double), typeof(string))]
public sealed class CompactNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is double number ? NumberFormatter.Compact(number) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

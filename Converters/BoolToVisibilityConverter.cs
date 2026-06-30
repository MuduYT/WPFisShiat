using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BibWpf.Converters;

/// <summary>
/// Boolescher Wert → <see cref="Visibility"/>. Wird z. B. verwendet, um
/// einen Error-Banner im Edit-Dialog anzuzeigen, wenn
/// <c>HasDbErrors == true</c>.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// <c>true</c> → Visible, <c>false</c> → Collapsed (Standard).
    /// Wird über den Parameter <c>"invert"</c> umgekehrt.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BibWpf.Converters;

/// <summary>
/// Gibt Visibility.Visible zurück, wenn der String nicht null/leer ist,
/// ansonsten Visibility.Collapsed.
/// </summary>
[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

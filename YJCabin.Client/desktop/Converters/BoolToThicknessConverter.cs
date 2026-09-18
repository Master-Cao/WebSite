using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace YJCabin.Desktop.Converters;

public sealed class BoolToThicknessConverter : IValueConverter
{
    public Thickness TrueThickness { get; set; }

    public Thickness FalseThickness { get; set; } = new(12);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? TrueThickness : FalseThickness;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

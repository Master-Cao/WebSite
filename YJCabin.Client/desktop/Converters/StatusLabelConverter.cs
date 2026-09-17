using System.Globalization;
using Avalonia.Data.Converters;

namespace YJCabin.Desktop.Converters;

public sealed class StatusLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            "Published" => "已发布",
            "Archived" => "已归档",
            "Draft" => "草稿",
            _ => value?.ToString() ?? ""
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using System.Collections.Frozen;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TrustSync.Desktop.Converters;

public static class CurrencyHelper
{
    private static readonly FrozenDictionary<string, string> Symbols = new Dictionary<string, string>
    {
        ["USD"] = "$",
        ["EUR"] = "€",
        ["GBP"] = "£",
        ["IQD"] = "ع.د",
        ["AED"] = "د.إ",
        ["SAR"] = "﷼",
        ["TRY"] = "₺",
        ["CAD"] = "CA$",
        ["AUD"] = "A$",
        ["JPY"] = "¥"
    }.ToFrozenDictionary();

    public static string ToSymbol(string? code)
        => code is not null && Symbols.TryGetValue(code, out var s) ? s : code ?? "";
}

public class CurrencySymbolConverter : IValueConverter
{
    public static readonly CurrencySymbolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => CurrencyHelper.ToSymbol(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

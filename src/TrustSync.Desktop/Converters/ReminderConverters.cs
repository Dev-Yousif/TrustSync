using System.Globalization;
using Avalonia.Data.Converters;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;

namespace TrustSync.Desktop.Converters;

public class ReminderScheduleConverter : IValueConverter
{
    public static readonly ReminderScheduleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Reminder r) return "";
        return r.RepeatType switch
        {
            RepeatType.Once => $"Once at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Every10Minutes => "Every 10 minutes",
            RepeatType.Every30Minutes => "Every 30 minutes",
            RepeatType.EveryHour => "Every hour",
            RepeatType.Daily => $"Daily at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Weekly => $"Weekly on {(DayOfWeek)(r.DayOfWeek ?? 0)} at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Monthly => $"Monthly on day {r.DayOfMonth ?? 1} at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Custom => $"Every {r.CustomIntervalMinutes ?? 60} min",
            _ => "Unknown"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ReminderNextFireConverter : IValueConverter
{
    public static readonly ReminderNextFireConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Reminder r) return "";
        if (!r.IsEnabled) return "Disabled";
        if (r.NextFireAt is null) return "Not scheduled";
        var local = r.NextFireAt.Value.ToLocalTime();
        var diff = local - DateTime.Now;
        if (diff.TotalMinutes < 1) return "Now";
        if (diff.TotalMinutes < 60) return $"In {(int)diff.TotalMinutes}m";
        if (diff.TotalHours < 24) return $"In {(int)diff.TotalHours}h {diff.Minutes}m";
        return local.ToString("MMM dd, hh:mm tt");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ReminderEnabledOpacityConverter : IValueConverter
{
    public static readonly ReminderEnabledOpacityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 1.0 : 0.5;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

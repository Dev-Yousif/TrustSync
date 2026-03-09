using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.Services;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class RemindersViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;

    [ObservableProperty] private ObservableCollection<Reminder> _reminders = [];

    // Form fields
    [ObservableProperty] private string _formTitle = string.Empty;
    [ObservableProperty] private string _formDescription = string.Empty;
    [ObservableProperty] private RepeatType _formRepeatType = RepeatType.Daily;
    [ObservableProperty] private string _formHourText = "20";
    [ObservableProperty] private string _formMinuteText = "00";
    [ObservableProperty] private int? _formDayOfWeek;
    [ObservableProperty] private int? _formDayOfMonth;
    [ObservableProperty] private int _formCustomMinutes = 60;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private int? _editingId;

    public RepeatType[] RepeatTypes { get; } = Enum.GetValues<RepeatType>();
    public int[] DaysOfMonth { get; } = Enumerable.Range(1, 31).ToArray();

    private int FormHour => int.TryParse(FormHourText, out var h) ? Math.Clamp(h, 0, 23) : 20;
    private int FormMinute => int.TryParse(FormMinuteText, out var m) ? Math.Clamp(m, 0, 59) : 0;

    public string[] DaysOfWeekNames { get; } =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    // Map between DayOfWeek name and index
    [ObservableProperty] private string? _formDayOfWeekName;

    partial void OnFormDayOfWeekNameChanged(string? value)
    {
        if (value is not null)
            FormDayOfWeek = Array.IndexOf(DaysOfWeekNames, value);
    }

    public RemindersViewModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Reminders = new ObservableCollection<Reminder>(await _reminderService.GetAllAsync());
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveReminderAsync()
    {
        if (string.IsNullOrWhiteSpace(FormTitle))
        {
            ShowToast("Title is required.", isError: true);
            return;
        }

        IsBusy = true;
        try
        {
            var reminder = new Reminder
            {
                Id = EditingId ?? 0,
                Title = FormTitle.Trim(),
                Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription.Trim(),
                IsEnabled = true,
                RepeatType = FormRepeatType,
                TimeOfDay = new TimeOnly(FormHour, FormMinute),
                DayOfWeek = FormRepeatType == RepeatType.Weekly ? FormDayOfWeek : null,
                DayOfMonth = FormRepeatType == RepeatType.Monthly ? FormDayOfMonth : null,
                CustomIntervalMinutes = FormRepeatType == RepeatType.Custom ? FormCustomMinutes : null,
            };

            var result = IsEditing
                ? await _reminderService.UpdateAsync(reminder)
                : await _reminderService.CreateAsync(reminder);

            if (result.IsSuccess)
            {
                ShowToast(IsEditing ? "Reminder updated." : "Reminder created.");
                ResetForm();
                await LoadAsync();
            }
            else
            {
                ShowToast(result.Error ?? "Failed to save reminder.", isError: true);
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void EditReminder(Reminder reminder)
    {
        IsEditing = true;
        EditingId = reminder.Id;
        FormTitle = reminder.Title;
        FormDescription = reminder.Description ?? string.Empty;
        FormRepeatType = reminder.RepeatType;
        FormHourText = reminder.TimeOfDay.Hour.ToString();
        FormMinuteText = reminder.TimeOfDay.Minute.ToString("D2");
        FormDayOfWeek = reminder.DayOfWeek;
        FormDayOfWeekName = reminder.DayOfWeek.HasValue && reminder.DayOfWeek.Value < 7
            ? DaysOfWeekNames[reminder.DayOfWeek.Value] : null;
        FormDayOfMonth = reminder.DayOfMonth;
        FormCustomMinutes = reminder.CustomIntervalMinutes ?? 60;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ResetForm();
    }

    [RelayCommand]
    private void RequestDeleteReminder(Reminder reminder)
    {
        ShowConfirmDialog("Delete Reminder", $"Are you sure you want to delete \"{reminder.Title}\"?", async () =>
        {
            IsBusy = true;
            try
            {
                var result = await _reminderService.DeleteAsync(reminder.Id);
                if (result.IsSuccess)
                {
                    await LoadAsync();
                    ShowToast("Reminder deleted.");
                }
                else
                    ShowToast(result.Error ?? "Failed to delete.", isError: true);
            }
            finally { IsBusy = false; }
        });
    }

    [RelayCommand]
    private async Task ToggleEnabledAsync(Reminder reminder)
    {
        reminder.IsEnabled = !reminder.IsEnabled;
        await _reminderService.UpdateAsync(reminder);
        await LoadAsync();
        ShowToast(reminder.IsEnabled ? $"\"{reminder.Title}\" enabled." : $"\"{reminder.Title}\" disabled.");
    }

    private void ResetForm()
    {
        IsEditing = false;
        EditingId = null;
        FormTitle = string.Empty;
        FormDescription = string.Empty;
        FormRepeatType = RepeatType.Daily;
        FormHourText = "20";
        FormMinuteText = "00";
        FormDayOfWeek = null;
        FormDayOfWeekName = null;
        FormDayOfMonth = null;
        FormCustomMinutes = 60;
    }

    public static string FormatSchedule(Reminder r)
    {
        return r.RepeatType switch
        {
            RepeatType.Once => $"Once at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Every10Minutes => "Every 10 minutes",
            RepeatType.Every30Minutes => "Every 30 minutes",
            RepeatType.EveryHour => "Every hour",
            RepeatType.Daily => $"Daily at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Weekly => $"Weekly on {(DayOfWeek)(r.DayOfWeek ?? 0)} at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Monthly => $"Monthly on day {r.DayOfMonth ?? 1} at {r.TimeOfDay:hh\\:mm tt}",
            RepeatType.Custom => $"Every {r.CustomIntervalMinutes ?? 60} minutes",
            _ => "Unknown"
        };
    }

    public static string FormatNextFire(Reminder r)
    {
        if (!r.IsEnabled) return "Disabled";
        if (r.NextFireAt is null) return "Not scheduled";
        var local = r.NextFireAt.Value.ToLocalTime();
        var diff = local - DateTime.Now;
        if (diff.TotalMinutes < 1) return "Now";
        if (diff.TotalMinutes < 60) return $"In {(int)diff.TotalMinutes}m";
        if (diff.TotalHours < 24) return $"In {(int)diff.TotalHours}h {diff.Minutes}m";
        return local.ToString("MMM dd, hh:mm tt");
    }
}

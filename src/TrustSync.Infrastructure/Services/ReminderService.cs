using Microsoft.EntityFrameworkCore;
using TrustSync.Application.Services;
using TrustSync.Domain.Common;
using TrustSync.Domain.Entities;
using TrustSync.Domain.Enums;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _db;

    public ReminderService(AppDbContext db) => _db = db;

    public async Task<List<Reminder>> GetAllAsync()
    {
        return await _db.Reminders
            .OrderBy(r => r.NextFireAt)
            .ToListAsync();
    }

    public async Task<Result> CreateAsync(Reminder reminder)
    {
        reminder.NextFireAt = CalculateNextFireAt(reminder);
        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(Reminder reminder)
    {
        var existing = await _db.Reminders.FindAsync(reminder.Id);
        if (existing is null) return Result.Failure("Reminder not found.");

        existing.Title = reminder.Title;
        existing.Description = reminder.Description;
        existing.IsEnabled = reminder.IsEnabled;
        existing.RepeatType = reminder.RepeatType;
        existing.CustomIntervalMinutes = reminder.CustomIntervalMinutes;
        existing.TimeOfDay = reminder.TimeOfDay;
        existing.DayOfWeek = reminder.DayOfWeek;
        existing.DayOfMonth = reminder.DayOfMonth;
        existing.NextFireAt = CalculateNextFireAt(existing);

        await _db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);
        if (reminder is null) return Result.Failure("Reminder not found.");

        _db.Reminders.Remove(reminder);
        await _db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<List<Reminder>> GetDueRemindersAsync()
    {
        var now = DateTime.UtcNow;
        return await _db.Reminders
            .Where(r => r.IsEnabled && r.NextFireAt != null && r.NextFireAt <= now)
            .ToListAsync();
    }

    public async Task MarkFiredAsync(int id)
    {
        var reminder = await _db.Reminders.FindAsync(id);
        if (reminder is null) return;

        reminder.LastFiredAt = DateTime.UtcNow;

        if (reminder.RepeatType == RepeatType.Once)
        {
            reminder.IsEnabled = false;
            reminder.NextFireAt = null;
        }
        else
        {
            reminder.NextFireAt = CalculateNextFireAt(reminder);
        }

        await _db.SaveChangesAsync();
    }

    public DateTime CalculateNextFireAt(Reminder reminder)
    {
        var now = DateTime.Now;
        var time = reminder.TimeOfDay;

        switch (reminder.RepeatType)
        {
            case RepeatType.Once:
                var onceTarget = now.Date.Add(time.ToTimeSpan());
                return (onceTarget > now ? onceTarget : onceTarget.AddDays(1)).ToUniversalTime();

            case RepeatType.Every10Minutes:
                return now.AddMinutes(10).ToUniversalTime();

            case RepeatType.Every30Minutes:
                return now.AddMinutes(30).ToUniversalTime();

            case RepeatType.EveryHour:
                return now.AddHours(1).ToUniversalTime();

            case RepeatType.Daily:
                var dailyTarget = now.Date.Add(time.ToTimeSpan());
                return (dailyTarget > now ? dailyTarget : dailyTarget.AddDays(1)).ToUniversalTime();

            case RepeatType.Weekly:
                var dow = (DayOfWeek)(reminder.DayOfWeek ?? 1);
                var daysUntil = ((int)dow - (int)now.DayOfWeek + 7) % 7;
                var weeklyTarget = now.Date.AddDays(daysUntil).Add(time.ToTimeSpan());
                if (weeklyTarget <= now) weeklyTarget = weeklyTarget.AddDays(7);
                return weeklyTarget.ToUniversalTime();

            case RepeatType.Monthly:
                var day = Math.Min(reminder.DayOfMonth ?? 1, DateTime.DaysInMonth(now.Year, now.Month));
                var monthlyTarget = new DateTime(now.Year, now.Month, day).Add(time.ToTimeSpan());
                if (monthlyTarget <= now)
                {
                    var next = now.AddMonths(1);
                    day = Math.Min(reminder.DayOfMonth ?? 1, DateTime.DaysInMonth(next.Year, next.Month));
                    monthlyTarget = new DateTime(next.Year, next.Month, day).Add(time.ToTimeSpan());
                }
                return monthlyTarget.ToUniversalTime();

            case RepeatType.Custom:
                var minutes = reminder.CustomIntervalMinutes ?? 60;
                return now.AddMinutes(minutes).ToUniversalTime();

            default:
                return now.AddDays(1).ToUniversalTime();
        }
    }
}

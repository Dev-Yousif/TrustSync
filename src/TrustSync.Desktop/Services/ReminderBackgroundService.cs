using System.Diagnostics;
using System.Timers;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TrustSync.Application.Services;

namespace TrustSync.Desktop.Services;

public sealed class ReminderBackgroundService : IDisposable
{
    private readonly IServiceProvider _services;
    private readonly System.Timers.Timer _timer;

    /// <summary>
    /// Fired on the background thread when a reminder is due.
    /// The UI layer subscribes to show in-app notification.
    /// </summary>
    public event EventHandler<ReminderNotification>? NotificationFired;

    public ReminderBackgroundService(IServiceProvider services)
    {
        _services = services;
        _timer = new System.Timers.Timer(30_000); // Check every 30 seconds
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private async void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            using var scope = _services.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            var dueReminders = await reminderService.GetDueRemindersAsync();

            foreach (var reminder in dueReminders)
            {
                // Notify UI thread
                NotificationFired?.Invoke(this, new ReminderNotification
                {
                    Title = reminder.Title,
                    Message = reminder.Description ?? reminder.Title
                });

                // Send OS-level notification with sound
                SendOsNotification(reminder.Title, reminder.Description ?? reminder.Title);

                // Mark as fired (calculates next fire time)
                await reminderService.MarkFiredAsync(reminder.Id);
            }
        }
        catch
        {
            // Don't crash the timer on errors
        }
    }

    private static string GetSoundFilePath()
    {
        var exeDir = AppContext.BaseDirectory;
        return Path.Combine(exeDir, "Assets", "reminder-sound.mp3");
    }

    private static string GetIconFilePath()
    {
        var exeDir = AppContext.BaseDirectory;
        return Path.Combine(exeDir, "app.ico");
    }

    private static void SendOsNotification(string title, string message)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var safeTitle = title.Replace("'", "''");
                var safeMessage = message.Replace("'", "''");
                var soundPath = GetSoundFilePath().Replace("'", "''");
                var iconPath = GetIconFilePath().Replace("'", "''");

                // Use NotifyIcon balloon tip with app icon and custom sound
                var script =
                    "Add-Type -AssemblyName System.Windows.Forms; " +
                    "Add-Type -AssemblyName System.Drawing; " +
                    "Add-Type -AssemblyName presentationCore; " +
                    $"$icon = New-Object System.Drawing.Icon('{iconPath}'); " +
                    "$n = New-Object System.Windows.Forms.NotifyIcon; " +
                    "$n.Icon = $icon; " +
                    "$n.BalloonTipIcon = 'None'; " +
                    $"$n.BalloonTipTitle = '{safeTitle}'; " +
                    $"$n.BalloonTipText = '{safeMessage}'; " +
                    "$n.Visible = $true; " +
                    "$n.ShowBalloonTip(10000); " +
                    $"$p = New-Object System.Windows.Media.MediaPlayer; " +
                    $"$p.Open([uri]'{soundPath}'); " +
                    "$p.Play(); " +
                    "Start-Sleep -Seconds 11; " +
                    "$p.Stop(); $p.Close(); " +
                    "$n.Dispose(); $icon.Dispose()";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            else if (OperatingSystem.IsLinux())
            {
                var safeTitle = title.Replace("\"", "\\\"");
                var safeMessage = message.Replace("\"", "\\\"");
                var soundPath = GetSoundFilePath();

                Process.Start(new ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--urgency=normal \"{safeTitle}\" \"{safeMessage}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                if (File.Exists(soundPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffplay",
                        Arguments = $"-nodisp -autoexit \"{soundPath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }
        }
        catch
        {
            // Notification delivery is best-effort
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}

public class ReminderNotification
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
}

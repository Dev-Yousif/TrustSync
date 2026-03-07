using YousifAccounting.Application.Security;

namespace YousifAccounting.Infrastructure.Security;

public sealed class SessionService : ISessionService
{
    private readonly object _lock = new();
    private int _autoLockTimeoutMinutes = 5;
    private System.Timers.Timer? _inactivityTimer;

    public bool IsAuthenticated { get; private set; }
    public DateTime? SessionStartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public string? MasterPassword { get; private set; }

    public event EventHandler? SessionExpired;
    public event EventHandler? SessionStarted;
    public event EventHandler? SessionEnded;

    public void StartSession(string masterPassword)
    {
        lock (_lock)
        {
            IsAuthenticated = true;
            MasterPassword = masterPassword;
            SessionStartedAt = DateTime.UtcNow;
            LastActivityAt = DateTime.UtcNow;

            _inactivityTimer?.Dispose();
            _inactivityTimer = new System.Timers.Timer(15_000); // check every 15s
            _inactivityTimer.Elapsed += OnInactivityCheck;
            _inactivityTimer.AutoReset = true;
            _inactivityTimer.Start();
        }

        SessionStarted?.Invoke(this, EventArgs.Empty);
    }

    public void RecordActivity()
    {
        lock (_lock)
        {
            if (IsAuthenticated)
                LastActivityAt = DateTime.UtcNow;
        }
    }

    public void EndSession()
    {
        lock (_lock)
        {
            IsAuthenticated = false;
            MasterPassword = null;
            _inactivityTimer?.Stop();
            _inactivityTimer?.Dispose();
            _inactivityTimer = null;
            SessionStartedAt = null;
        }

        SessionEnded?.Invoke(this, EventArgs.Empty);
    }

    public bool IsSessionExpired()
    {
        lock (_lock)
        {
            if (!IsAuthenticated) return true;
            if (_autoLockTimeoutMinutes <= 0) return false;
            var timeout = TimeSpan.FromMinutes(_autoLockTimeoutMinutes);
            return DateTime.UtcNow - LastActivityAt > timeout;
        }
    }

    public int GetAutoLockTimeoutMinutes() => _autoLockTimeoutMinutes;

    public void SetAutoLockTimeoutMinutes(int minutes)
    {
        _autoLockTimeoutMinutes = Math.Max(0, minutes);
    }

    private void OnInactivityCheck(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (IsSessionExpired())
        {
            EndSession();
            SessionExpired?.Invoke(this, EventArgs.Empty);
        }
    }
}

namespace TrustSync.Application.Security;

public interface ISessionService
{
    bool IsAuthenticated { get; }
    DateTime? SessionStartedAt { get; }
    DateTime LastActivityAt { get; }
    string? MasterPassword { get; }

    void StartSession(string masterPassword);
    void EndSession();
    void RecordActivity();
    bool IsSessionExpired();
    int GetAutoLockTimeoutMinutes();
    void SetAutoLockTimeoutMinutes(int minutes);

    event EventHandler? SessionExpired;
    event EventHandler? SessionStarted;
    event EventHandler? SessionEnded;
}

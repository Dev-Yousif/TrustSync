using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Security;

public interface IAuthenticationService
{
    Task<bool> IsFirstRunRequiredAsync();
    Task<Result> SetupMasterPasswordAsync(string displayName, string password, string defaultCurrency);
    Task<Result> LoginAsync(string password);
    Task<Result> ChangePasswordAsync(string currentPassword, string newPassword);
}

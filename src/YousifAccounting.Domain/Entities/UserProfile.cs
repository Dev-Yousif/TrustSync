using YousifAccounting.Domain.Common;

namespace YousifAccounting.Domain.Entities;

public sealed class UserProfile : BaseEntity
{
    public string DisplayName { get; set; } = "User";
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string HashAlgorithm { get; set; } = "Argon2id";
    public int HashVersion { get; set; } = 0x13;
    public int HashDegreeOfParallelism { get; set; }
    public int HashMemorySize { get; set; }
    public int HashIterations { get; set; }
    public string DefaultCurrencyCode { get; set; } = "USD";
    public int AutoLockTimeoutMinutes { get; set; } = 5;
    public DateTime? LastLoginAt { get; set; }

    public Currency DefaultCurrency { get; set; } = null!;
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YousifAccounting.Application.Security;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Security;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private readonly ISecurityService _securityService;
    private readonly ISessionService _sessionService;
    private readonly PasswordValidator _passwordValidator;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IAuditService _auditService;

    public AuthenticationService(
        AppDbContext context,
        ISecurityService securityService,
        ISessionService sessionService,
        PasswordValidator passwordValidator,
        ILogger<AuthenticationService> logger,
        IAuditService auditService)
    {
        _context = context;
        _securityService = securityService;
        _sessionService = sessionService;
        _passwordValidator = passwordValidator;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<bool> IsFirstRunRequiredAsync()
    {
        return !await _context.UserProfiles.AnyAsync();
    }

    public async Task<Result> SetupMasterPasswordAsync(string displayName, string password, string defaultCurrency)
    {
        if (await _context.UserProfiles.AnyAsync())
            return Result.Failure("Master password already configured.");

        var validation = _passwordValidator.Validate(password);
        if (!validation.IsValid)
            return Result.Failure(string.Join(" ", validation.Errors));

        var hashResult = _securityService.HashPassword(password);

        var profile = new UserProfile
        {
            DisplayName = displayName,
            PasswordHash = hashResult.Hash,
            PasswordSalt = hashResult.Salt,
            HashAlgorithm = hashResult.Algorithm,
            HashVersion = hashResult.Version,
            HashDegreeOfParallelism = hashResult.DegreeOfParallelism,
            HashMemorySize = hashResult.MemorySize,
            HashIterations = hashResult.Iterations,
            DefaultCurrencyCode = defaultCurrency,
            AutoLockTimeoutMinutes = 5,
            LastLoginAt = DateTime.UtcNow
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        _sessionService.StartSession(password);
        _logger.LogInformation("Master password configured for user {DisplayName}", displayName);
        await _auditService.LogAsync(AuditAction.Create, "UserProfile", profile.Id, $"Master password configured for {displayName}");

        return Result.Success();
    }

    public async Task<Result> LoginAsync(string password)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync();
        if (profile is null)
            return Result.Failure("No user profile found. Setup required.");

        var storedHash = new PasswordHashResult
        {
            Hash = profile.PasswordHash,
            Salt = profile.PasswordSalt,
            Algorithm = profile.HashAlgorithm,
            Version = profile.HashVersion,
            DegreeOfParallelism = profile.HashDegreeOfParallelism,
            MemorySize = profile.HashMemorySize,
            Iterations = profile.HashIterations
        };

        if (!_securityService.VerifyPassword(password, storedHash))
        {
            _logger.LogWarning("Failed login attempt");
            return Result.Failure("Incorrect password.");
        }

        profile.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _sessionService.SetAutoLockTimeoutMinutes(profile.AutoLockTimeoutMinutes);
        _sessionService.StartSession(password);

        _logger.LogInformation("User logged in successfully");
        await _auditService.LogAsync(AuditAction.Login, "UserProfile", profile.Id);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync();
        if (profile is null)
            return Result.Failure("No user profile found.");

        var storedHash = new PasswordHashResult
        {
            Hash = profile.PasswordHash,
            Salt = profile.PasswordSalt,
            Algorithm = profile.HashAlgorithm,
            Version = profile.HashVersion,
            DegreeOfParallelism = profile.HashDegreeOfParallelism,
            MemorySize = profile.HashMemorySize,
            Iterations = profile.HashIterations
        };

        if (!_securityService.VerifyPassword(currentPassword, storedHash))
            return Result.Failure("Current password is incorrect.");

        var validation = _passwordValidator.Validate(newPassword);
        if (!validation.IsValid)
            return Result.Failure(string.Join(" ", validation.Errors));

        var newHashResult = _securityService.HashPassword(newPassword);

        profile.PasswordHash = newHashResult.Hash;
        profile.PasswordSalt = newHashResult.Salt;
        profile.HashAlgorithm = newHashResult.Algorithm;
        profile.HashVersion = newHashResult.Version;
        profile.HashDegreeOfParallelism = newHashResult.DegreeOfParallelism;
        profile.HashMemorySize = newHashResult.MemorySize;
        profile.HashIterations = newHashResult.Iterations;

        await _context.SaveChangesAsync();

        _sessionService.StartSession(newPassword);
        _logger.LogInformation("Password changed successfully");
        await _auditService.LogAsync(AuditAction.PasswordChange, "UserProfile", profile.Id);

        return Result.Success();
    }
}

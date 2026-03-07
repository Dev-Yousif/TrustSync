namespace YousifAccounting.Application.Security;

public sealed class PasswordValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}

public sealed class PasswordValidator
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    public PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return new PasswordValidationResult { IsValid = false, Errors = errors };
        }

        if (password.Length < MinLength)
            errors.Add($"Password must be at least {MinLength} characters.");

        if (password.Length > MaxLength)
            errors.Add($"Password must not exceed {MaxLength} characters.");

        if (!password.Any(char.IsUpper))
            errors.Add("Must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            errors.Add("Must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            errors.Add("Must contain at least one digit.");

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Must contain at least one special character.");

        return new PasswordValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

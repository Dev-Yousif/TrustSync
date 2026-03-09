namespace TrustSync.Application.Security;

public sealed class PasswordHashResult
{
    public required string Hash { get; init; }
    public required string Salt { get; init; }
    public string Algorithm { get; init; } = "Argon2id";
    public int Version { get; init; } = 0x13;
    public int DegreeOfParallelism { get; init; }
    public int MemorySize { get; init; }
    public int Iterations { get; init; }
}

using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using YousifAccounting.Application.Security;

namespace YousifAccounting.Infrastructure.Security;

public sealed class SecurityService : ISecurityService
{
    private const int DegreeOfParallelism = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 3;
    private const int HashLength = 32;
    private const int SaltLength = 16;

    public PasswordHashResult HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var hash = argon2.GetBytes(HashLength);

        return new PasswordHashResult
        {
            Hash = Convert.ToBase64String(hash),
            Salt = Convert.ToBase64String(salt),
            Algorithm = "Argon2id",
            Version = 0x13,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };
    }

    public bool VerifyPassword(string password, PasswordHashResult stored)
    {
        var salt = Convert.FromBase64String(stored.Salt);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = stored.DegreeOfParallelism,
            MemorySize = stored.MemorySize,
            Iterations = stored.Iterations
        };

        var computedHash = argon2.GetBytes(HashLength);
        var storedHash = Convert.FromBase64String(stored.Hash);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    public byte[] DeriveEncryptionKey(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(32);
    }
}

namespace YousifAccounting.Application.Security;

public interface ISecurityService
{
    PasswordHashResult HashPassword(string password);
    bool VerifyPassword(string password, PasswordHashResult stored);
    byte[] DeriveEncryptionKey(string password, byte[] salt);
}

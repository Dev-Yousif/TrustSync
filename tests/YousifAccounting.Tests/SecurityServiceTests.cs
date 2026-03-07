using FluentAssertions;
using Xunit;
using YousifAccounting.Application.Security;
using YousifAccounting.Infrastructure.Security;

namespace YousifAccounting.Tests;

public class SecurityServiceTests
{
    private readonly SecurityService _service = new();

    [Fact]
    public void HashPassword_Returns_Valid_Result()
    {
        var result = _service.HashPassword("TestPassword1!");

        result.Hash.Should().NotBeNullOrEmpty();
        result.Salt.Should().NotBeNullOrEmpty();
        result.Algorithm.Should().Be("Argon2id");
        result.DegreeOfParallelism.Should().BeGreaterThan(0);
        result.MemorySize.Should().BeGreaterThan(0);
        result.Iterations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void HashPassword_Produces_Different_Salts()
    {
        var result1 = _service.HashPassword("TestPassword1!");
        var result2 = _service.HashPassword("TestPassword1!");

        result1.Salt.Should().NotBe(result2.Salt);
        result1.Hash.Should().NotBe(result2.Hash);
    }

    [Fact]
    public void VerifyPassword_Returns_True_For_Correct_Password()
    {
        var hash = _service.HashPassword("TestPassword1!");
        var verified = _service.VerifyPassword("TestPassword1!", hash);
        verified.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Returns_False_For_Wrong_Password()
    {
        var hash = _service.HashPassword("TestPassword1!");
        var verified = _service.VerifyPassword("WrongPassword!", hash);
        verified.Should().BeFalse();
    }

    [Fact]
    public void DeriveEncryptionKey_Returns_32_Bytes()
    {
        var salt = new byte[16];
        var key = _service.DeriveEncryptionKey("TestPassword1!", salt);
        key.Should().HaveCount(32);
    }

    [Fact]
    public void DeriveEncryptionKey_Same_Input_Produces_Same_Output()
    {
        var salt = new byte[16];
        var key1 = _service.DeriveEncryptionKey("TestPassword1!", salt);
        var key2 = _service.DeriveEncryptionKey("TestPassword1!", salt);
        key1.Should().BeEquivalentTo(key2);
    }
}

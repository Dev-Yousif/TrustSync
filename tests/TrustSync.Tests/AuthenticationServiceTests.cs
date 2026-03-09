using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using TrustSync.Application.Security;
using TrustSync.Infrastructure.Security;

namespace TrustSync.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<ISessionService> _sessionMock = new();
    private readonly SecurityService _securityService = new();
    private readonly PasswordValidator _passwordValidator = new();

    private AuthenticationService CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.Create(dbName);
        return new AuthenticationService(
            db, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());
    }

    [Fact]
    public async Task IsFirstRunRequired_Returns_True_When_No_Users()
    {
        var service = CreateService();
        var result = await service.IsFirstRunRequiredAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetupMasterPassword_Succeeds_With_Valid_Password()
    {
        var service = CreateService();
        var result = await service.SetupMasterPasswordAsync("Test User", "MyStr0ng!Pass", "USD");

        result.IsSuccess.Should().BeTrue();
        _sessionMock.Verify(s => s.StartSession("MyStr0ng!Pass"), Times.Once);
    }

    [Fact]
    public async Task SetupMasterPassword_Fails_With_Weak_Password()
    {
        var service = CreateService();
        var result = await service.SetupMasterPasswordAsync("Test User", "weak", "USD");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SetupMasterPassword_Fails_If_Already_Configured()
    {
        var dbName = Guid.NewGuid().ToString();
        var service1 = CreateService(dbName);
        await service1.SetupMasterPasswordAsync("User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.SetupMasterPasswordAsync("User2", "MyStr0ng!Pass2", "USD");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already configured");
    }

    [Fact]
    public async Task IsFirstRunRequired_Returns_False_After_Setup()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        await service.SetupMasterPasswordAsync("Test User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.IsFirstRunRequiredAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Login_Succeeds_With_Correct_Password()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        await service.SetupMasterPasswordAsync("User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.LoginAsync("MyStr0ng!Pass");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Fails_With_Wrong_Password()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        await service.SetupMasterPasswordAsync("User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.LoginAsync("WrongPassword!");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Incorrect");
    }

    [Fact]
    public async Task ChangePassword_Succeeds_With_Valid_Credentials()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        await service.SetupMasterPasswordAsync("User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.ChangePasswordAsync("MyStr0ng!Pass", "NewStr0ng!Pass");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_Fails_With_Wrong_Current_Password()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        await service.SetupMasterPasswordAsync("User", "MyStr0ng!Pass", "USD");

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new AuthenticationService(
            db2, _securityService, _sessionMock.Object, _passwordValidator,
            NullLogger<AuthenticationService>.Instance, new NullAuditService());

        var result = await service2.ChangePasswordAsync("WrongPass!", "NewStr0ng!Pass");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("incorrect");
    }
}

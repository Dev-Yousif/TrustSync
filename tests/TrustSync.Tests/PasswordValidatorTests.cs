using FluentAssertions;
using Xunit;
using TrustSync.Application.Security;

namespace TrustSync.Tests;

public class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [Fact]
    public void Valid_Password_Returns_Success()
    {
        var result = _validator.Validate("MyStr0ng!Pass");
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_Or_Null_Password_Fails(string? password)
    {
        var result = _validator.Validate(password!);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void Short_Password_Fails()
    {
        var result = _validator.Validate("Ab1!short");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least"));
    }

    [Fact]
    public void No_Uppercase_Fails()
    {
        var result = _validator.Validate("mystr0ng!pass");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase"));
    }

    [Fact]
    public void No_Lowercase_Fails()
    {
        var result = _validator.Validate("MYSTR0NG!PASS");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase"));
    }

    [Fact]
    public void No_Digit_Fails()
    {
        var result = _validator.Validate("MyStrong!Pass");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public void No_Special_Character_Fails()
    {
        var result = _validator.Validate("MyStr0ngPass1");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special"));
    }

    [Fact]
    public void Multiple_Violations_Returns_All_Errors()
    {
        var result = _validator.Validate("short");
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThan(1);
    }
}

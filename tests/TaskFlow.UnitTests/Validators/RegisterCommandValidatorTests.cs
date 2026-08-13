using FluentAssertions;
using TaskFlow.Application.Features.Auth.Register;

namespace TaskFlow.UnitTests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new RegisterCommand("Victor", "victor@example.com", "StrongPass123"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithWeakPassword_NoNumber_ShouldFail()
    {
        var result = _validator.Validate(new RegisterCommand(null, "victor@example.com", "onlyletters"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldFail()
    {
        var result = _validator.Validate(new RegisterCommand(null, "victor@example.com", "Ab1"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var result = _validator.Validate(new RegisterCommand(null, "not-an-email", "StrongPass123"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithoutName_ShouldPass()
    {
        var result = _validator.Validate(new RegisterCommand(null, "victor@example.com", "StrongPass123"));

        result.IsValid.Should().BeTrue();
    }
}

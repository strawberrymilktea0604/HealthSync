using HealthSync.Application.Commands;
using HealthSync.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace HealthSync.Application.Tests.Validators;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Pass_When_AllFieldsValid()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_EmailEmpty()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "", // Empty
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email là bắt buộc");
    }

    [Fact]
    public void Should_Fail_When_EmailInvalidFormat()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "invalid-email", // Invalid format
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Định dạng email không hợp lệ");
    }

    [Fact]
    public void Should_Fail_When_EmailWhitespace()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "   ", // Whitespace only
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email là bắt buộc");
    }

    [Fact]
    public void Should_Fail_When_PasswordEmpty()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "", // Empty
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Mật khẩu là bắt buộc");
    }

    [Fact]
    public void Should_Fail_When_PasswordTooShort()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "1234567", // 7 chars, too short
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Mật khẩu phải có ít nhất 8 ký tự");
    }

    [Fact]
    public void Should_Pass_When_PasswordMinimumLength()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "12345678", // Exactly 8 chars
            VerificationCode = "123456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_VerificationCodeEmpty()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "" // Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationCode)
              .WithErrorMessage("Mã xác thực là bắt buộc");
    }

    [Fact]
    public void Should_Fail_When_VerificationCodeTooShort()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "12345" // 5 chars, too short
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationCode)
              .WithErrorMessage("Mã xác thực phải có 6 ký tự");
    }

    [Fact]
    public void Should_Fail_When_VerificationCodeTooLong()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "1234567" // 7 chars, too long
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VerificationCode)
              .WithErrorMessage("Mã xác thực phải có 6 ký tự");
    }

    [Fact]
    public void Should_Pass_When_VerificationCodeCorrectLength()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456" // Exactly 6 chars
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VerificationCode);
    }

    [Fact]
    public void Should_Fail_MultipleValidationErrors()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "", // Invalid
            Password = "123", // Too short
            VerificationCode = "12" // Too short
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.VerificationCode);
        Assert.Equal(4, result.Errors.Count);
    }
}
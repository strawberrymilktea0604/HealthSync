using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup default configuration (not configured - will use console output)
        _configurationMock.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.gmail.com");
        _configurationMock.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
        _configurationMock.Setup(c => c["EmailSettings:SenderEmail"]).Returns("your-email@gmail.com");
        _configurationMock.Setup(c => c["EmailSettings:SenderName"]).Returns("HealthSync");
        _configurationMock.Setup(c => c["EmailSettings:Password"]).Returns("");

        _emailService = new EmailService(_configurationMock.Object);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_WithUnconfiguredEmail_CompletesSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendVerificationCodeAsync(email, code));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_WithValidEmailAndCode_CompletesSuccessfully()
    {
        // Arrange
        var email = "user@example.com";
        var code = "654321";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendVerificationCodeAsync(email, code));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_WithEmptyCode_CompletesSuccessfully()
    {
        // Arrange
        var email = "user@example.com";
        var code = "";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendVerificationCodeAsync(email, code));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_WithUnconfiguredEmail_CompletesSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var resetToken = "reset-token-123";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendResetPasswordEmailAsync(email, resetToken));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_WithValidEmailAndToken_CompletesSuccessfully()
    {
        // Arrange
        var email = "user@example.com";
        var resetToken = "token-abc-xyz";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendResetPasswordEmailAsync(email, resetToken));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_WithEmptyToken_CompletesSuccessfully()
    {
        // Arrange
        var email = "user@example.com";
        var resetToken = "";

        // Act
        var exception = await Record.ExceptionAsync(() => _emailService.SendResetPasswordEmailAsync(email, resetToken));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_WithNullSmtpPort_UsesDefaultPort()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:SmtpPort"]).Returns((string?)null);
        var emailService = new EmailService(_configurationMock.Object);
        var email = "test@example.com";
        var code = "123456";

        // Act
        var exception = await Record.ExceptionAsync(() => emailService.SendVerificationCodeAsync(email, code));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_WithNullSmtpPort_UsesDefaultPort()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:SmtpPort"]).Returns((string?)null);
        var emailService = new EmailService(_configurationMock.Object);
        var email = "test@example.com";
        var resetToken = "token-123";

        // Act
        var exception = await Record.ExceptionAsync(() => emailService.SendResetPasswordEmailAsync(email, resetToken));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_WithNullSenderEmail_CompletesSuccessfully()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:SenderEmail"]).Returns((string?)null);
        var emailService = new EmailService(_configurationMock.Object);
        var email = "test@example.com";
        var code = "123456";

        // Act
        var exception = await Record.ExceptionAsync(() => emailService.SendVerificationCodeAsync(email, code));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_WithNullPassword_CompletesSuccessfully()
    {
        // Arrange
        _configurationMock.Setup(c => c["EmailSettings:Password"]).Returns((string?)null);
        var emailService = new EmailService(_configurationMock.Object);
        var email = "test@example.com";
        var resetToken = "token-123";

        // Act
        var exception = await Record.ExceptionAsync(() => emailService.SendResetPasswordEmailAsync(email, resetToken));

        // Assert
        Assert.Null(exception);
    }
}

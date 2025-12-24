using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _handler = new ForgotPasswordCommandHandler(
            _contextMock.Object,
            _jwtTokenServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokenAndSendEmail_WhenEmailExists()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "user@test.com" }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _jwtTokenServiceMock.Setup(j => j.GenerateResetTokenAsync(1, "user@test.com"))
            .ReturnsAsync("reset-token-123");

        _emailServiceMock.Setup(e => e.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ForgotPasswordCommand { Email = "user@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtTokenServiceMock.Verify(j => j.GenerateResetTokenAsync(1, "user@test.com"), Times.Once);
        _emailServiceMock.Verify(e => e.SendResetPasswordEmailAsync("user@test.com", "reset-token-123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSendEmail_WhenEmailDoesNotExist()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new ForgotPasswordCommand { Email = "notfound@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtTokenServiceMock.Verify(j => j.GenerateResetTokenAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotThrowException_WhenEmailDoesNotExist()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new ForgotPasswordCommand { Email = "notfound@test.com" };

        // Act
        var exception = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Null(exception); // Should not throw for security reasons
    }

    [Fact]
    public async Task Handle_ShouldGenerateToken_WithCorrectUserIdAndEmail()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 42, Email = "specific@test.com" }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _jwtTokenServiceMock.Setup(j => j.GenerateResetTokenAsync(42, "specific@test.com"))
            .ReturnsAsync("token-for-user-42");

        _emailServiceMock.Setup(e => e.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ForgotPasswordCommand { Email = "specific@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtTokenServiceMock.Verify(j => j.GenerateResetTokenAsync(42, "specific@test.com"), Times.Once);
        _emailServiceMock.Verify(e => e.SendResetPasswordEmailAsync("specific@test.com", "token-for-user-42"), Times.Once);
    }
}

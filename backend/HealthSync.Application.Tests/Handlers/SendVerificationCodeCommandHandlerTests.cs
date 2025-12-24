using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class SendVerificationCodeCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly SendVerificationCodeCommandHandler _handler;

    public SendVerificationCodeCommandHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new SendVerificationCodeCommandHandler(_emailServiceMock.Object, _contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndSendCode_WhenEmailNotExists()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new SendVerificationCodeCommand { Email = "newuser@test.com" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync("newuser@test.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "existing@test.com" }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new SendVerificationCodeCommand { Email = "existing@test.com" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Email đã tồn tại trong hệ thống!", exception.Message);
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldBeCaseInsensitive_WhenCheckingEmailExists()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "EXISTING@TEST.COM" }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new SendVerificationCodeCommand { Email = "existing@test.com" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Email đã tồn tại trong hệ thống!", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldGenerate6DigitCode_WhenSendingVerification()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        string? capturedCode = null;
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((email, code) => capturedCode = code)
            .Returns(Task.CompletedTask);

        var command = new SendVerificationCodeCommand { Email = "test@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedCode);
        Assert.Equal(6, capturedCode.Length);
        Assert.True(int.TryParse(capturedCode, out int codeValue));
        Assert.InRange(codeValue, 100000, 999999);
    }

    [Fact]
    public async Task Handle_ShouldStoreCodeInVerificationStore_WhenSendingCode()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        string? capturedCode = null;
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((email, code) => capturedCode = code)
            .Returns(Task.CompletedTask);

        var command = new SendVerificationCodeCommand { Email = "test@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedCode);
        var isValid = VerificationCodeStore.Verify(command.Email, capturedCode, markAsUsed: false);
        Assert.True(isValid);
    }

    [Fact]
    public async Task Handle_ShouldCallEmailService_WithCorrectParameters()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new SendVerificationCodeCommand { Email = "user@example.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync(
            "user@example.com",
            It.Is<string>(code => code.Length == 6)),
            Times.Once);
    }
}

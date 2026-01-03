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
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _otpServiceMock = new Mock<IOtpService>();
        _emailServiceMock = new Mock<IEmailService>();
        _handler = new ForgotPasswordCommandHandler(
            _contextMock.Object,
            _otpServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateOtpAndSendEmail_WhenEmailExists()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "user@test.com", IsActive = true }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _otpServiceMock.Setup(j => j.GenerateOtp("user@test.com"))
            .Returns("123456");

        _emailServiceMock.Setup(e => e.SendResetPasswordOtpAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ForgotPasswordCommand { Email = "user@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(j => j.GenerateOtp("user@test.com"), Times.Once);
        _emailServiceMock.Verify(e => e.SendResetPasswordOtpAsync("user@test.com", "123456"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailDoesNotExist()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new ForgotPasswordCommand { Email = "notfound@test.com" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}

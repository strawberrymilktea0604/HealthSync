using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class ResendResetOtpCommandHandlerTests
{
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly ResendResetOtpCommandHandler _handler;

    public ResendResetOtpCommandHandlerTests()
    {
        _otpServiceMock = new Mock<IOtpService>();
        _emailServiceMock = new Mock<IEmailService>();
        _handler = new ResendResetOtpCommandHandler(_otpServiceMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateOtpAndSendEmail()
    {
        // Arrange
        _otpServiceMock.Setup(o => o.GenerateOtp("user@test.com"))
            .Returns("123456");
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendResetOtpCommand { Email = "user@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(o => o.GenerateOtp("user@test.com"), Times.Once);
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync("user@test.com", "123456"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendGeneratedOtpToEmail()
    {
        // Arrange
        var email = "test@example.com";
        var generatedOtp = "654321";
        _otpServiceMock.Setup(o => o.GenerateOtp(email)).Returns(generatedOtp);
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(email, generatedOtp))
            .Returns(Task.CompletedTask);

        var command = new ResendResetOtpCommand { Email = email };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync(email, generatedOtp), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewOtpEachTime()
    {
        // Arrange
        var callCount = 0;
        _otpServiceMock.Setup(o => o.GenerateOtp(It.IsAny<string>()))
            .Returns(() => (++callCount).ToString().PadLeft(6, '0'));
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendResetOtpCommand { Email = "user@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(o => o.GenerateOtp("user@test.com"), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldCallServicesInCorrectOrder()
    {
        // Arrange
        var sequence = new List<string>();
        _otpServiceMock.Setup(o => o.GenerateOtp(It.IsAny<string>()))
            .Returns("123456")
            .Callback(() => sequence.Add("GenerateOtp"));
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .Callback(() => sequence.Add("SendEmail"));

        var command = new ResendResetOtpCommand { Email = "user@test.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, sequence.Count);
        Assert.Equal("GenerateOtp", sequence[0]);
        Assert.Equal("SendEmail", sequence[1]);
    }
}

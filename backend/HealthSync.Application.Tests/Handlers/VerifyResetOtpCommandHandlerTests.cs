using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class VerifyResetOtpCommandHandlerTests
{
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly VerifyResetOtpCommandHandler _handler;

    public VerifyResetOtpCommandHandlerTests()
    {
        _otpServiceMock = new Mock<IOtpService>();
        _handler = new VerifyResetOtpCommandHandler(_otpServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenOtpIsValid()
    {
        // Arrange
        _otpServiceMock.Setup(o => o.ValidateOtp("user@test.com", "123456"))
            .Returns(true);

        var command = new VerifyResetOtpCommand { Email = "user@test.com", Otp = "123456" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _otpServiceMock.Verify(o => o.ValidateOtp("user@test.com", "123456"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOtpIsInvalid()
    {
        // Arrange
        _otpServiceMock.Setup(o => o.ValidateOtp("user@test.com", "wrong-otp"))
            .Returns(false);

        var command = new VerifyResetOtpCommand { Email = "user@test.com", Otp = "wrong-otp" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _otpServiceMock.Verify(o => o.ValidateOtp("user@test.com", "wrong-otp"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOtpExpired()
    {
        // Arrange
        _otpServiceMock.Setup(o => o.ValidateOtp("user@test.com", "expired-otp"))
            .Returns(false);

        var command = new VerifyResetOtpCommand { Email = "user@test.com", Otp = "expired-otp" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldCallOtpServiceWithCorrectParameters()
    {
        // Arrange
        var email = "test@example.com";
        var otp = "654321";
        _otpServiceMock.Setup(o => o.ValidateOtp(email, otp)).Returns(true);

        var command = new VerifyResetOtpCommand { Email = email, Otp = otp };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(o => o.ValidateOtp(email, otp), Times.Once);
    }
}

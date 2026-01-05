using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class VerifyResetOtpCommandHandlerTests
{
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly VerifyResetOtpCommandHandler _handler;

    public VerifyResetOtpCommandHandlerTests()
    {
        _otpServiceMock = new Mock<IOtpService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        
        _handler = new VerifyResetOtpCommandHandler(
            _otpServiceMock.Object,
            _contextMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithToken_WhenOtpIsValid()
    {
        // Arrange
        var email = "user@test.com";
        var otp = "123456";
        var userId = 1;
        var token = "reset-token";

        _otpServiceMock.Setup(o => o.ValidateOtp(email, otp)).Returns(true);
        
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = userId, Email = email }
        };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _jwtTokenServiceMock.Setup(j => j.GenerateResetTokenAsync(userId, email)).ReturnsAsync(token);

        var command = new VerifyResetOtpCommand { Email = email, Otp = otp };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(token, result.Token);
        _otpServiceMock.Verify(o => o.ValidateOtp(email, otp), Times.Once);
        _jwtTokenServiceMock.Verify(j => j.GenerateResetTokenAsync(userId, email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOtpIsInvalid()
    {
        // Arrange
        var email = "user@test.com";
        var otp = "wrong-otp";
        
        _otpServiceMock.Setup(o => o.ValidateOtp(email, otp)).Returns(false);

        var command = new VerifyResetOtpCommand { Email = email, Otp = otp };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.True(string.IsNullOrEmpty(result.Token)); // Token is null or empty
    }
}

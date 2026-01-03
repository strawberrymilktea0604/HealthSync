using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class ResendResetOtpCommandHandlerTests
{
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ResendResetOtpCommandHandler _handler;

    public ResendResetOtpCommandHandlerTests()
    {
        _otpServiceMock = new Mock<IOtpService>();
        _emailServiceMock = new Mock<IEmailService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ResendResetOtpCommandHandler(_otpServiceMock.Object, _emailServiceMock.Object, _contextMock.Object);
        
        // Setup default user for context to avoid NRE in tests if handler uses it
        // The handler uses FirstOrDefaultAsync, which MockQueryable can handle
    }

    private void SetupContext(string email, bool isActive = true)
    {
         var users = new List<ApplicationUser>
         {
             new ApplicationUser { UserId = 1, Email = email, IsActive = isActive }
         };
         var mockUsers = users.AsQueryable().BuildMock();
         _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);
    }
    
    [Fact]
    public async Task Handle_ShouldGenerateOtpAndSendEmail()
    {
        // Arrange
        var email = "user@test.com";
        SetupContext(email);        _otpServiceMock.Setup(o => o.GenerateOtp("user@test.com"))
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
        SetupContext(email);
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
        var email = "user@test.com";
        SetupContext(email);
        var callCount = 0;
        _otpServiceMock.Setup(o => o.GenerateOtp(It.IsAny<string>()))
            .Returns(() => (++callCount).ToString().PadLeft(6, '0'));
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendResetOtpCommand { Email = email };

        // Act
        await _handler.Handle(command, CancellationToken.None);
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _otpServiceMock.Verify(o => o.GenerateOtp(email), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldCallServicesInCorrectOrder()
    {
        // Arrange
        var email = "user@test.com";
        SetupContext(email);
        var sequence = new List<string>();
        _otpServiceMock.Setup(o => o.GenerateOtp(It.IsAny<string>()))
            .Returns("123456")
            .Callback(() => sequence.Add("GenerateOtp"));
        _emailServiceMock.Setup(e => e.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .Callback(() => sequence.Add("SendEmail"));

        var command = new ResendResetOtpCommand { Email = email };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, sequence.Count);

        Assert.Equal("GenerateOtp", sequence[0]);
        Assert.Equal("SendEmail", sequence[1]);
    }
}

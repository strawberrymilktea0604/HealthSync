using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class VerifyEmailCodeCommandHandlerTests
{
    private readonly VerifyEmailCodeCommandHandler _handler;

    public VerifyEmailCodeCommandHandlerTests()
    {
        _handler = new VerifyEmailCodeCommandHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenCodeIsValid()
    {
        // Arrange
        var email = "test@test.com";
        var code = "123456";
        VerificationCodeStore.Store(email, code, TimeSpan.FromMinutes(5));

        var command = new VerifyEmailCodeCommand { Email = email, Code = code };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCodeIsInvalid()
    {
        // Arrange
        var email = "test@test.com";
        VerificationCodeStore.Store(email, "123456", TimeSpan.FromMinutes(5));

        var command = new VerifyEmailCodeCommand { Email = email, Code = "654321" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenEmailNotFound()
    {
        // Arrange
        var command = new VerifyEmailCodeCommand { Email = "notfound@test.com", Code = "123456" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldRemoveCodeAfterVerification_WhenCodeIsValid()
    {
        // Arrange
        var email = "test@test.com";
        var code = "123456";
        VerificationCodeStore.Store(email, code, TimeSpan.FromMinutes(5));

        var command = new VerifyEmailCodeCommand { Email = email, Code = code };

        // Act
        var firstResult = await _handler.Handle(command, CancellationToken.None);
        var secondResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(firstResult);
        Assert.False(secondResult); // Code should be removed after first verification
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCodeIsExpired()
    {
        // Arrange
        var email = "test@test.com";
        var code = "123456";
        VerificationCodeStore.Store(email, code, TimeSpan.FromMilliseconds(1));
        
        // Wait for code to expire
        await Task.Delay(100);

        var command = new VerifyEmailCodeCommand { Email = email, Code = code };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}

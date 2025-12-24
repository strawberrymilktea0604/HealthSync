using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetGoogleAndroidClientIdQueryHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly GetGoogleAndroidClientIdQueryHandler _handler;

    public GetGoogleAndroidClientIdQueryHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _handler = new GetGoogleAndroidClientIdQueryHandler(_googleAuthServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAndroidClientId()
    {
        // Arrange
        var expectedClientId = "123456789-abcdefg.apps.googleusercontent.com";
        _googleAuthServiceMock.Setup(s => s.GetAndroidClientIdAsync())
            .ReturnsAsync(expectedClientId);

        var query = new GetGoogleAndroidClientIdQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedClientId, result);
        _googleAuthServiceMock.Verify(s => s.GetAndroidClientIdAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallGoogleAuthService()
    {
        // Arrange
        _googleAuthServiceMock.Setup(s => s.GetAndroidClientIdAsync())
            .ReturnsAsync("client-id");

        var query = new GetGoogleAndroidClientIdQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _googleAuthServiceMock.Verify(s => s.GetAndroidClientIdAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSameClientId_OnMultipleCalls()
    {
        // Arrange
        var clientId = "consistent-client-id";
        _googleAuthServiceMock.Setup(s => s.GetAndroidClientIdAsync())
            .ReturnsAsync(clientId);

        var query = new GetGoogleAndroidClientIdQuery();

        // Act
        var result1 = await _handler.Handle(query, CancellationToken.None);
        var result2 = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(clientId, result1);
    }
}

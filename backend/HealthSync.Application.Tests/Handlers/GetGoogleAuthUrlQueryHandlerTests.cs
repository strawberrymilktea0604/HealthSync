using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetGoogleAuthUrlQueryHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly GetGoogleAuthUrlQueryHandler _handler;

    public GetGoogleAuthUrlQueryHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _handler = new GetGoogleAuthUrlQueryHandler(_googleAuthServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthUrl_WhenCalled()
    {
        // Arrange
        var expectedUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=xxx&redirect_uri=yyy&state=abc123";
        _googleAuthServiceMock.Setup(s => s.GetAuthorizationUrl("abc123"))
            .ReturnsAsync(expectedUrl);

        var query = new GetGoogleAuthUrlQuery { State = "abc123" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedUrl, result);
        _googleAuthServiceMock.Verify(s => s.GetAuthorizationUrl("abc123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassStateParameter()
    {
        // Arrange
        var state = "custom-state-value";
        _googleAuthServiceMock.Setup(s => s.GetAuthorizationUrl(state))
            .ReturnsAsync("https://google.com/auth");

        var query = new GetGoogleAuthUrlQuery { State = state };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _googleAuthServiceMock.Verify(s => s.GetAuthorizationUrl(state), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDifferentUrls_ForDifferentStates()
    {
        // Arrange
        _googleAuthServiceMock.Setup(s => s.GetAuthorizationUrl("state1"))
            .ReturnsAsync("https://google.com/auth?state=state1");
        _googleAuthServiceMock.Setup(s => s.GetAuthorizationUrl("state2"))
            .ReturnsAsync("https://google.com/auth?state=state2");

        var query1 = new GetGoogleAuthUrlQuery { State = "state1" };
        var query2 = new GetGoogleAuthUrlQuery { State = "state2" };

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.Contains("state1", result1);
        Assert.Contains("state2", result2);
    }
}

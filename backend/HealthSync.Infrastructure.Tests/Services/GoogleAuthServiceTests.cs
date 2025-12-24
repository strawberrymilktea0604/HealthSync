using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HealthSync.Infrastructure.Tests.Services;

public class GoogleAuthServiceTests
{
    [Fact]
    public async Task GetAuthorizationUrl_WithValidConfiguration_ReturnsUrl()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns("test-client-id");
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_AUTH_URI"]).Returns("https://accounts.google.com/o/oauth2/v2/auth");

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);
        var state = "test-state";

        // Act
        var url = await service.GetAuthorizationUrl(state);

        // Assert
        Assert.NotNull(url);
        Assert.Contains("test-client-id", url);
        Assert.Contains("callback", url);
        Assert.Contains("test-state", url);
        Assert.Contains("openid", url);
    }

    [Fact]
    public async Task GetAuthorizationUrl_WithMissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns((string?)null);
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_AUTH_URI"]).Returns("https://accounts.google.com/o/oauth2/v2/auth");

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GetAuthorizationUrl("state");
        });
    }

    [Fact]
    public async Task GetAuthorizationUrl_WithMissingRedirectUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns("test-client-id");
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns((string?)null);
        configurationMock.Setup(c => c["GOOGLE_REDIRECT_URI"]).Returns((string?)null);
        configurationMock.Setup(c => c["GOOGLE_AUTH_URI"]).Returns("https://accounts.google.com/o/oauth2/v2/auth");

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GetAuthorizationUrl("state");
        });
    }

    [Fact]
    public async Task GetAuthorizationUrl_WithMissingAuthUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns("test-client-id");
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_AUTH_URI"]).Returns((string?)null);

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GetAuthorizationUrl("state");
        });
    }

    [Fact]
    public async Task GetAuthorizationUrl_WithSpecialCharactersInState_EncodesCorrectly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns("test-client-id");
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_AUTH_URI"]).Returns("https://accounts.google.com/o/oauth2/v2/auth");

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);
        var state = "test state with spaces & special=chars";

        // Act
        var url = await service.GetAuthorizationUrl(state);

        // Assert
        Assert.NotNull(url);
        Assert.Contains("state=", url);
        Assert.DoesNotContain("test state with spaces", url); // Should be URL encoded
    }

    [Fact]
    public async Task ProcessCallbackAsync_WithMissingTokenUri_ReturnsNull()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["GoogleOAuth:ClientId"]).Returns("test-client-id");
        configurationMock.Setup(c => c["GoogleOAuth:ClientSecret"]).Returns("test-secret");
        configurationMock.Setup(c => c["GoogleOAuth:RedirectUri"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_REDIRECT_URI"]).Returns("http://localhost/callback");
        configurationMock.Setup(c => c["GOOGLE_TOKEN_URI"]).Returns((string?)null);

        var httpClient = new HttpClient();
        var service = new GoogleAuthService(configurationMock.Object, httpClient);

        // Act
        var result = await service.ProcessCallbackAsync("test-code");

        // Assert - Should return null when exception occurs
        Assert.Null(result);
    }


}

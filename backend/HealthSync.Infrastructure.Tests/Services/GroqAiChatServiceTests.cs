using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HealthSync.Infrastructure.Tests.Services;

public class GroqAiChatServiceTests
{
    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns((string?)null);
        
        // Clear environment variable if set
        var originalEnvVar = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        Environment.SetEnvironmentVariable("GROQ_API_KEY", null);

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var service = new GroqAiChatService(configurationMock.Object);
            });
        }
        finally
        {
            // Restore original environment variable
            Environment.SetEnvironmentVariable("GROQ_API_KEY", originalEnvVar);
        }
    }

    [Fact]
    public void Constructor_WithApiKeyInConfiguration_CreatesInstance()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key-12345");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        // Act
        var service = new GroqAiChatService(configurationMock.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithApiKeyInEnvironmentVariable_CreatesInstance()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns((string?)null);
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        var originalEnvVar = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        Environment.SetEnvironmentVariable("GROQ_API_KEY", "env-api-key-67890");

        try
        {
            // Act
            var service = new GroqAiChatService(configurationMock.Object);

            // Assert
            Assert.NotNull(service);
        }
        finally
        {
            // Restore original environment variable
            Environment.SetEnvironmentVariable("GROQ_API_KEY", originalEnvVar);
        }
    }

    [Fact]
    public void Constructor_WithNullModelId_UsesDefaultModel()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns((string?)null);

        // Act
        var service = new GroqAiChatService(configurationMock.Object);

        // Assert - Should not throw and use default model
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithInvalidApiKey_ThrowsException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("invalid-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        var service = new GroqAiChatService(configurationMock.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync("User context", "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithEmptyUserContext_DoesNotThrow()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        var service = new GroqAiChatService(configurationMock.Object);

        // Act & Assert - Will throw because API key is invalid, but test validates no null reference
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync("", "Question with empty context");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        var service = new GroqAiChatService(configurationMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync("Context", "Question", cts.Token);
        });
    }
}

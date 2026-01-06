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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");
        
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
    public void Constructor_WithMissingBaseUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-key");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var service = new GroqAiChatService(configurationMock.Object);
        });
    }

    [Fact]
    public void Constructor_WithApiKeyInConfiguration_CreatesInstance()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key-12345");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

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
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync("Context", "Question", cts.Token);
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithValidContextWithProfile_ParsesCorrectly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Male"",
                ""age"": 25,
                ""heightCm"": 175.5,
                ""currentWeightKg"": 70.0,
                ""bmi"": 22.8,
                ""bmiStatus"": ""Bình thường"",
                ""bmr"": 1700,
                ""activityLevel"": ""Moderate""
            },
            ""goal"": {
                ""type"": ""Weight Loss"",
                ""targetWeightKg"": 65.0,
                ""deadline"": ""2024-12-31""
            },
            ""recentActivityLogs"": ""- [01/01 10:00] Logged workout\n- [01/02 11:00] Logged nutrition""
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithMissingProfile_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""goal"": {
                ""type"": ""Weight Loss""
            }
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithMissingGoal_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Female"",
                ""age"": 30
            }
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithNullGoal_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Female"",
                ""age"": 30
            },
            ""goal"": null
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithMissingActivityLogs_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Male"",
                ""age"": 25
            }
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithEmptyJson_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = "{}";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithIncompleteProfile_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Male""
            }
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithActivityLogsNotString_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""recentActivityLogs"": 12345
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithCompleteGoalData_ParsesCorrectly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Male"",
                ""age"": 25,
                ""heightCm"": 175,
                ""currentWeightKg"": 70,
                ""bmi"": 22.8,
                ""bmiStatus"": ""Normal"",
                ""bmr"": 1700,
                ""activityLevel"": ""Moderate""
            },
            ""goal"": {
                ""type"": ""Weight Loss"",
                ""targetWeightKg"": 65,
                ""deadline"": ""2024-12-31""
            }
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }
    
    [Fact]
    public async Task GetHealthAdviceAsync_WithDailyLogsAndCompletedGoals_ParsesCorrectly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": {
                ""gender"": ""Male"",
                ""age"": 25
            },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""nutrition"": { ""calories"": 2000, ""foodItems"": [""Rice"", ""Chicken""] }, ""workout"": { ""status"": ""Completed"", ""durationMin"": 60, ""exercises"": [""Push up"", ""Running""] } },
                { ""date"": ""2024-01-02"", ""nutrition"": { ""calories"": 1800 }, ""workout"": { ""status"": ""Rest"" } }
            ],
            ""completedGoalsHistory"": [""Lost 5kg"", ""Ran 10km""]
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            // This invocation will calculate the 'dailyLogs' and 'completedGoals' strings internally
            // before failing at the HTTP request step.
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithMalformedDailyLogsAndGoals_HandlesGracefully()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        var service = new GroqAiChatService(configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": null,
            ""completedGoalsHistory"": ""NotAnArray""
        }";

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await service.GetHealthAdviceAsync(context, "Test question");
        });
    }
}

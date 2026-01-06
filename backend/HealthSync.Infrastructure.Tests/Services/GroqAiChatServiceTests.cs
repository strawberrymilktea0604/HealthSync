using System.Net;
using System.Text;
using System.Text.Json;
using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace HealthSync.Infrastructure.Tests.Services;

public class GroqAiChatServiceTests
{
    private readonly HttpClient _validHttpClient;

    public GroqAiChatServiceTests()
    {
        _validHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.groq.com/openai/v1/")
        };
        _validHttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-key");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new GroqAiChatService(null!, configurationMock.Object);
        });
    }

    [Fact]
    public void Constructor_WithHttpClientMissingBaseAddress_AndMissingConfig_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns((string?)null);
        var httpClient = new HttpClient(); // No BaseAddress

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            new GroqAiChatService(httpClient, configurationMock.Object);
        });
    }

    [Fact]
    public void Constructor_WithHttpClientMissingBaseAddress_ButConfigHasUrl_SetsBaseAddress()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1/");
        var httpClient = new HttpClient(); // No BaseAddress

        // Act
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        // Assert
        Assert.NotNull(httpClient.BaseAddress);
        Assert.Equal("https://api.groq.com/openai/v1/", httpClient.BaseAddress.ToString());
    }

    [Fact]
    public void Constructor_WithValidHttpClient_CreatesInstance()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        // Act
        var service = new GroqAiChatService(_validHttpClient, configurationMock.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullModelId_UsesDefaultModel()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns((string?)null);

        // Act
        var service = new GroqAiChatService(_validHttpClient, configurationMock.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetHealthAdviceAsync_ReturnsContent_WhenApiCallSucceeds()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Groq:ModelId"]).Returns("groq-beta");

        var expectedResponse = "Here is some health advice.";
        var apiResponse = new
        {
            choices = new[]
            {
                new { message = new { content = expectedResponse } }
            }
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.groq.com/openai/v1/")
        };

        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""goal"": { ""type"": ""Leisure"" }
        }";

        // Act
        var result = await service.GetHealthAdviceAsync(context, "Help me");

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithValidContextWithProfile_ParsesCorrectly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var service = new GroqAiChatService(_validHttpClient, configurationMock.Object);

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
        
        // Cannot easily check internal logic without mocking HttpClient handler to capture request.
        // But we can assert it doesn't throw before the request.
        // To verify prompt, we need to inspect the HttpRequestMessage sent.
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""choices"":[{""message"":{""content"":""OK""}}]}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var serviceWithMock = new GroqAiChatService(httpClient, configurationMock.Object);

        // Act
        await serviceWithMock.GetHealthAdviceAsync(context, "Question");

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Content != null && 
                req.Content.ReadAsStringAsync().Result.Contains("Male") // Check if context was injected
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithMissingProfile_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{ ""goal"": { ""type"": ""Weight Loss"" } }";

        await service.GetHealthAdviceAsync(context, "Question");
        
        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithNullGoal_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{ ""profile"": { ""gender"": ""Male"" }, ""goal"": null }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithDailyLogsAndCompletedGoals_ParsesCorrectly()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""nutrition"": { ""calories"": 2000, ""foodItems"": [""Rice"", ""Chicken""] }, ""workout"": { ""status"": ""Completed"", ""durationMin"": 60, ""exercises"": [""Push up"", ""Running""] } }
            ],
            ""completedGoalsHistory"": [""Lost 5kg"", ""Ran 10km""]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Content != null && 
                req.Content.ReadAsStringAsync().Result.Contains("Ran 10km") &&
                req.Content.ReadAsStringAsync().Result.Contains("Push up")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public void Constructor_WithExistingAuthorizationHeader_DoesNotOverride()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.groq.com/openai/v1/")
        };
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer existing-key");

        // Act
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        // Assert
        Assert.NotNull(service);
        Assert.Equal("Bearer existing-key", httpClient.DefaultRequestHeaders.Authorization?.ToString());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request")
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GetHealthAdviceAsync("{}", "Question")
        );
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WhenApiReturnsNullContent_ReturnsDefaultMessage()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""choices"":[]}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        // Act
        var result = await service.GetHealthAdviceAsync("{}", "Question");

        // Assert
        Assert.Equal("Xin lỗi, tôi không thể xử lý câu hỏi của bạn lúc này. Vui lòng thử lại sau. 🙏", result);
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithEmptyContext_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{}";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithNutritionButNoFoodItems_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""nutrition"": { ""calories"": 1500 } }
            ]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithWorkoutRestDay_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Female"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""workout"": { ""status"": ""Rest"" } }
            ]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify(
            "SendAsync", 
            Times.Once(), 
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Content != null && 
                req.Content.ReadAsStringAsync().Result.Contains("Nghỉ ngơi")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithWorkoutButNoExercises_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""workout"": { ""status"": ""Completed"", ""durationMin"": 30 } }
            ]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithInvalidWorkoutStatus_HandlesAsRest()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""workout"": { ""status"": """" } }
            ]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify(
            "SendAsync", 
            Times.Once(), 
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Content != null && 
                req.Content.ReadAsStringAsync().Result.Contains("Nghỉ ngơi")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetHealthAdviceAsync_WithDayWithoutWorkout_HandlesGracefully()
    {
        var configurationMock = new Mock<IConfiguration>();
        var handlerMock = SetupMockHandler();
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.groq.com") };
        var service = new GroqAiChatService(httpClient, configurationMock.Object);

        var context = @"{
            ""profile"": { ""gender"": ""Male"" },
            ""recentLogsLast7Days"": [
                { ""date"": ""2024-01-01"", ""nutrition"": { ""calories"": 2000 } }
            ]
        }";

        await service.GetHealthAdviceAsync(context, "Question");

        handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
    
    private Mock<HttpMessageHandler> SetupMockHandler()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""choices"":[{""message"":{""content"":""OK""}}]}", Encoding.UTF8, "application/json")
            });
        return handlerMock;
    }
}

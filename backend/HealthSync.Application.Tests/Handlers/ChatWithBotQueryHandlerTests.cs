using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers
{
    public class ChatWithBotQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<IAiChatService> _mockAiChatService;
        private readonly ChatWithBotQueryHandler _handler;

        public ChatWithBotQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockAiChatService = new Mock<IAiChatService>();
            _handler = new ChatWithBotQueryHandler(_mockContext.Object, _mockAiChatService.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsAiResponse()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What should I eat today?"
            };

            SetupBasicMocks();

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("You should eat a balanced diet with proteins and vegetables.");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("You should eat a balanced diet with proteins and vegetables.", result.Response);
            Assert.NotEqual(Guid.Empty, result.MessageId);
            _mockContext.Verify(c => c.Add(It.IsAny<ChatMessage>()), Times.Exactly(2)); // User + Assistant messages
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SavesUserMessageBeforeAiCall()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "How many calories should I consume?"
            };

            SetupBasicMocks();

            ChatMessage? capturedUserMessage = null;
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>()))
                .Callback<object>(msg =>
                {
                    if (msg is ChatMessage chatMsg && chatMsg.Role == "user")
                    {
                        capturedUserMessage = chatMsg;
                    }
                });

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Based on your profile, aim for 2000 calories.");

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedUserMessage);
            Assert.Equal("user", capturedUserMessage.Role);
            Assert.Equal("How many calories should I consume?", capturedUserMessage.Content);
            Assert.Equal(1, capturedUserMessage.UserId);
        }

        [Fact]
        public async Task Handle_SavesAssistantMessageAfterAiCall()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What exercises should I do?"
            };

            SetupBasicMocks();

            ChatMessage? capturedAssistantMessage = null;
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>()))
                .Callback<object>(msg =>
                {
                    if (msg is ChatMessage chatMsg && chatMsg.Role == "assistant")
                    {
                        capturedAssistantMessage = chatMsg;
                    }
                });

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("I recommend starting with cardio exercises.");

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedAssistantMessage);
            Assert.Equal("assistant", capturedAssistantMessage.Role);
            Assert.Equal("I recommend starting with cardio exercises.", capturedAssistantMessage.Content);
            Assert.Equal(1, capturedAssistantMessage.UserId);
        }

        [Fact]
        public async Task Handle_CallsAiServiceWithQuestionAndContext()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test question"
            };

            SetupBasicMocks();

            bool aiServiceCalled = false;
            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((context, question, ct) =>
                {
                    aiServiceCalled = true;
                    Assert.Equal("Test question", question);
                    Assert.NotNull(context);
                })
                .ReturnsAsync("AI response");

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(aiServiceCalled);
        }

        private void SetupBasicMocks()
        {
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>())).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_WithUserHavingNoProfile_ShouldReturnResponseWithoutProfileContext()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What should I eat?"
            };

            // Setup mocks with no profile
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>())).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Please complete your profile first.");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Please complete your profile first.", result.Response);
            _mockContext.Verify(c => c.Add(It.IsAny<ChatMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithUserHavingNoGoals_ShouldReturnResponseWithoutGoalContext()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "How am I progressing?"
            };

            // Setup mocks with profile but no goals
            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Male",
                Dob = new DateTime(1990, 1, 1),
                WeightKg = 75,
                HeightCm = 175,
                ActivityLevel = "Moderate"
            };

            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>())).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Set a goal first to track your progress.");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Set a goal first to track your progress.", result.Response);
        }

        [Fact]
        public async Task Handle_WithUserHavingNoActivityHistory_ShouldReturnResponseWithEmptyLogs()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What should I do today?"
            };

            // Setup mocks with profile and goal but no workout/nutrition logs
            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Female",
                Dob = new DateTime(1995, 6, 15),
                WeightKg = 60,
                HeightCm = 165,
                ActivityLevel = "Light"
            };

            var goal = new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "Weight Loss",
                TargetValue = 55,
                Status = "in_progress",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                ProgressRecords = new List<ProgressRecord>()
            };

            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal> { goal }.AsQueryable().BuildMock());
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
            _mockContext.Setup(c => c.Add(It.IsAny<ChatMessage>())).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Start with a light cardio session.");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Start with a light cardio session.", result.Response);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}


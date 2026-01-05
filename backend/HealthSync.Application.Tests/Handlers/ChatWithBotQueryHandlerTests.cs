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
            var user = new ApplicationUser { UserId = 1, Email = "test@example.com" };
            _mockContext.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.UserActionLogs).Returns(new List<UserActionLog>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.FoodItems).Returns(new List<FoodItem>().AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Exercises).Returns(new List<Exercise>().AsQueryable().BuildMockDbSet().Object);
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
            SetupBasicMocks();

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

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);

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

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal> { goal }.AsQueryable().BuildMockDbSet().Object);

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

        [Fact]
        public async Task Handle_WithNonExistentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 999, // User does not exist
                Question = "Test question"
            };

            // Setup empty users list (user does not exist)
            _mockContext.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser>().AsQueryable().BuildMockDbSet().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _handler.Handle(query, CancellationToken.None));
            
            Assert.Equal("User account no longer exists or has been deleted.", exception.Message);
            
            // Verify that no messages were saved
            _mockContext.Verify(c => c.Add(It.IsAny<ChatMessage>()), Times.Never);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithProfileAndGoalWithProgressRecords_UpdatesCurrentWeight()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "How am I doing?"
            };

            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Male",
                Dob = new DateTime(1990, 5, 15),
                WeightKg = 80,
                HeightCm = 175,
                ActivityLevel = "Moderate"
            };

            var goal = new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "Weight Loss",
                TargetValue = 70,
                Status = "in_progress",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(1),
                ProgressRecords = new List<ProgressRecord>
                {
                    new ProgressRecord { ProgressRecordId = 1, GoalId = 1, WeightKg = 75, RecordDate = DateTime.UtcNow.AddDays(-5) },
                    new ProgressRecord { ProgressRecordId = 2, GoalId = 1, WeightKg = 74, RecordDate = DateTime.UtcNow.AddDays(-1) }
                }
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal> { goal }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Great progress! Keep it up!");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Great progress! Keep it up!", result.Response);
        }

        [Fact]
        public async Task Handle_WithCompletedGoals_IncludesGoalHistory()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What have I achieved?"
            };

            var completedGoal1 = new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "Weight Loss",
                TargetValue = 75,
                Status = "completed",
                StartDate = DateTime.UtcNow.AddMonths(-6),
                EndDate = DateTime.UtcNow.AddMonths(-3),
                ProgressRecords = new List<ProgressRecord>()
            };

            var completedGoal2 = new Goal
            {
                GoalId = 2,
                UserId = 1,
                Type = "Muscle Gain",
                TargetValue = 80,
                Status = "completed",
                StartDate = DateTime.UtcNow.AddMonths(-12),
                EndDate = DateTime.UtcNow.AddMonths(-9),
                ProgressRecords = new List<ProgressRecord>()
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal> { completedGoal1, completedGoal2 }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("You've achieved amazing results!");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithUserActionLogs_IncludesActivityLogs()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "What did I do recently?"
            };

            var actionLogs = new List<UserActionLog>
            {
                new UserActionLog { Id = 1, UserId = 1, ActionType = "CreateWorkoutLog", Description = "Logged workout", Timestamp = DateTime.UtcNow.AddHours(-2) },
                new UserActionLog { Id = 2, UserId = 1, ActionType = "CreateNutritionLog", Description = "Logged meal", Timestamp = DateTime.UtcNow.AddHours(-1) }
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserActionLogs).Returns(actionLogs.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("You've been active today!");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithNutritionAndWorkoutLogs_BuildsDailyContext()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "How's my week looking?"
            };

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var foodItem = new FoodItem { FoodItemId = 1, Name = "Chicken Breast" };
            var exercise = new Exercise { ExerciseId = 1, Name = "Push-ups", MuscleGroup = "Chest" };

            var nutritionLog = new NutritionLog
            {
                NutritionLogId = 1,
                UserId = 1,
                LogDate = today,
                FoodEntries = new List<FoodEntry>
                {
                    new FoodEntry
                    {
                        FoodEntryId = 1,
                        FoodItemId = 1,
                        FoodItem = foodItem,
                        CaloriesKcal = 200,
                        ProteinG = 40,
                        CarbsG = 0,
                        FatG = 5
                    }
                }
            };

            var workoutLog = new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = yesterday,
                DurationMin = 60,
                Notes = "Good session",
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseSessionId = 1,
                        ExerciseId = 1,
                        Exercise = exercise
                    }
                }
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog> { nutritionLog }.AsQueryable().BuildMockDbSet().Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog> { workoutLog }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Great week!");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithFemaleProfile_CalculatesBMRCorrectly()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test"
            };

            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Female",
                Dob = new DateTime(1995, 3, 10),
                WeightKg = 60,
                HeightCm = 165,
                ActivityLevel = "Light"
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithUnderweightBMI_GetsBMIStatus()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test"
            };

            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Male",
                Dob = new DateTime(2000, 1, 1),
                WeightKg = 50,  // Low weight for BMI < 18.5
                HeightCm = 175,
                ActivityLevel = "Light"
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithOverweightBMI_GetsBMIStatus()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test"
            };

            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Male",
                Dob = new DateTime(2000, 1, 1),
                WeightKg = 80,  // BMI between 25-30
                HeightCm = 170,
                ActivityLevel = "Light"
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithObeseBMI_GetsBMIStatus()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test"
            };

            var profile = new UserProfile
            {
                UserId = 1,
                Gender = "Male",
                Dob = new DateTime(2000, 1, 1),
                WeightKg = 100,  // BMI >= 30
                HeightCm = 170,
                ActivityLevel = "Light"
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.UserProfiles).Returns(new List<UserProfile> { profile }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WithGoalStatusActive_IncludesGoal()
        {
            // Arrange
            var query = new ChatWithBotQuery
            {
                UserId = 1,
                Question = "Test"
            };

            var goal = new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "Muscle Gain",
                TargetValue = 85,
                Status = "active",  // Test "active" status
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(2),
                ProgressRecords = new List<ProgressRecord>()
            };

            SetupBasicMocks();
            _mockContext.Setup(c => c.Goals).Returns(new List<Goal> { goal }.AsQueryable().BuildMockDbSet().Object);

            _mockAiChatService.Setup(a => a.GetHealthAdviceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}


using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers
{
    public class GetCustomerDashboardQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetCustomerDashboardQueryHandler _handler;

        public GetCustomerDashboardQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetCustomerDashboardQueryHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var query = new GetCustomerDashboardQuery { UserId = 1 };
            var emptyUsers = new List<ApplicationUser>().AsQueryable();

            var mockUserSet = CreateMockDbSet(emptyUsers);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _handler.Handle(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_UserWithoutProfile_ReturnsEmailAsFullName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = null,
                Goals = new List<Goal>()
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UserInfo);
            Assert.Equal("test@example.com", result.UserInfo.FullName);
        }

        [Fact]
        public async Task Handle_UserWithActiveGoal_ReturnsGoalProgress()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User", WeightKg = 70 },
                Goals = new List<Goal>
                {
                    new Goal
                    {
                        GoalId = 1,
                        Type = "Weight Loss",
                        Status = "active",
                        TargetValue = 65,
                        EndDate = DateTime.UtcNow.AddDays(30),
                        ProgressRecords = new List<ProgressRecord>
                        {
                            new ProgressRecord { RecordDate = DateTime.UtcNow.AddDays(-5), Value = 68, WeightKg = 68 },
                            new ProgressRecord { RecordDate = DateTime.UtcNow.AddDays(-1), Value = 67, WeightKg = 67 }
                        }
                    }
                }
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GoalProgress);
            Assert.Equal("Weight Loss", result.GoalProgress.GoalType);
            Assert.Equal("active", result.GoalProgress.Status);
            Assert.NotNull(result.WeightProgress);
            Assert.Equal(2, result.WeightProgress.WeightHistory.Count);
        }

        [Fact]
        public async Task Handle_UserWithoutActiveGoal_ReturnsDefaultGoalProgress()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User" },
                Goals = new List<Goal>()
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GoalProgress);
            Assert.Equal("None", result.GoalProgress.GoalType);
            Assert.Equal("No active goal", result.GoalProgress.Status);
        }

        [Fact]
        public async Task Handle_TodayNutritionLog_ReturnsCaloriesConsumed()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User" },
                Goals = new List<Goal>()
            };

            var todayLog = new NutritionLog
            {
                UserId = 1,
                LogDate = DateTime.UtcNow.AddHours(7).Date,
                FoodEntries = new List<FoodEntry>
                {
                    new FoodEntry { CaloriesKcal = 200 },
                    new FoodEntry { CaloriesKcal = 300 },
                    new FoodEntry { CaloriesKcal = 150 }
                }
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var nutritionLogs = new List<NutritionLog> { todayLog }.AsQueryable();

            var mockUserSet = CreateMockDbSet(users);
            var mockNutritionSet = CreateMockDbSet(nutritionLogs);

            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(mockNutritionSet.Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.TodayStats);
            Assert.Equal(650, result.TodayStats.CaloriesConsumed);
        }

        [Fact]
        public async Task Handle_TodayWorkoutLog_ReturnsWorkoutDuration()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User" },
                Goals = new List<Goal>()
            };

            var todayWorkout = new WorkoutLog
            {
                UserId = 1,
                WorkoutDate = DateTime.UtcNow.AddHours(7).Date,
                DurationMin = 45
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var workoutLogs = new List<WorkoutLog> { todayWorkout }.AsQueryable();

            var mockUserSet = CreateMockDbSet(users);
            var mockWorkoutSet = CreateMockDbSet(workoutLogs);

            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(mockWorkoutSet.Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.TodayStats);
            Assert.Equal("45 min", result.TodayStats.WorkoutDuration);
        }

        [Fact]
        public async Task Handle_ConsecutiveWorkoutDays_CalculatesCorrectStreak()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User" },
                Goals = new List<Goal>()
            };

            var today = DateTime.UtcNow.Date;
            var workoutLogs = new List<WorkoutLog>
            {
                new WorkoutLog { UserId = 1, WorkoutDate = today },
                new WorkoutLog { UserId = 1, WorkoutDate = today.AddDays(-1) },
                new WorkoutLog { UserId = 1, WorkoutDate = today.AddDays(-2) }
            }.AsQueryable();

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var mockUserSet = CreateMockDbSet(users);
            var mockWorkoutSet = CreateMockDbSet(workoutLogs);

            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(mockWorkoutSet.Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ExerciseStreak);
            Assert.Equal(3, result.ExerciseStreak.CurrentStreak);
        }

        [Fact]
        public async Task Handle_NoWorkouts_ReturnsZeroStreak()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserId = 1,
                Email = "test@example.com",
                Profile = new UserProfile { FullName = "Test User" },
                Goals = new List<Goal>()
            };

            var users = new List<ApplicationUser> { user }.AsQueryable();
            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);

            var query = new GetCustomerDashboardQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ExerciseStreak);
            Assert.Equal(0, result.ExerciseStreak.CurrentStreak);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            return data.BuildMockDbSet();
        }
    }
}

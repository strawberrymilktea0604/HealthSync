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
    public class GetAdminStatisticsQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly GetAdminStatisticsQueryHandler _handler;

        public GetAdminStatisticsQueryHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _handler = new GetAdminStatisticsQueryHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCompleteAdminStatistics()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = 30 };
            SetupMockData();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UserStatistics);
            Assert.NotNull(result.WorkoutStatistics);
            Assert.NotNull(result.NutritionStatistics);
            Assert.NotNull(result.GoalStatistics);
        }

        [Fact]
        public async Task Handle_UserStatistics_ReturnsCorrectTotals()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = 365 };
            var users = new List<ApplicationUser>
            {
                new ApplicationUser 
                { 
                    UserId = 1, 
                    Email = "user1@test.com", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UserRoles = new List<UserRole> 
                    { 
                        new UserRole { Role = new Role { RoleName = "Customer" } } 
                    }
                },
                new ApplicationUser 
                { 
                    UserId = 2, 
                    Email = "user2@test.com", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UserRoles = new List<UserRole> 
                    { 
                        new UserRole { Role = new Role { RoleName = "Customer" } } 
                    }
                },
                new ApplicationUser 
                { 
                    UserId = 3, 
                    Email = "admin@test.com", 
                    IsActive = false, 
                    CreatedAt = DateTime.UtcNow.AddDays(-40),
                    UserRoles = new List<UserRole> 
                    { 
                        new UserRole { Role = new Role { RoleName = "Admin" } } 
                    }
                }
            }.AsQueryable();

            var mockUserSet = CreateMockDbSet(users);
            _mockContext.Setup(c => c.ApplicationUsers).Returns(mockUserSet.Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Exercises).Returns(CreateMockDbSet(new List<Exercise>().AsQueryable()).Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.FoodItems).Returns(CreateMockDbSet(new List<FoodItem>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Goals).Returns(CreateMockDbSet(new List<Goal>().AsQueryable()).Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.UserStatistics.TotalUsers);
            Assert.Equal(2, result.UserStatistics.ActiveUsers);
            Assert.Equal(1, result.UserStatistics.NewUsersThisWeek);
            Assert.Equal(2, result.UserStatistics.NewUsersThisMonth);
        }

        [Fact]
        public async Task Handle_WorkoutStatistics_ReturnsTopExercises()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = 365 };
            
            var exercise1 = new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest" };
            var exercise2 = new Exercise { ExerciseId = 2, Name = "Squat", MuscleGroup = "Legs" };

            var workoutLogs = new List<WorkoutLog>
            {
                new WorkoutLog
                {
                    WorkoutLogId = 1,
                    WorkoutDate = DateTime.UtcNow.AddDays(-5),
                    ExerciseSessions = new List<ExerciseSession>
                    {
                        new ExerciseSession { ExerciseId = 1, Exercise = exercise1 },
                        new ExerciseSession { ExerciseId = 1, Exercise = exercise1 },
                        new ExerciseSession { ExerciseId = 2, Exercise = exercise2 }
                    }
                },
                new WorkoutLog
                {
                    WorkoutLogId = 2,
                    WorkoutDate = DateTime.UtcNow.AddDays(-10),
                    ExerciseSessions = new List<ExerciseSession>
                    {
                        new ExerciseSession { ExerciseId = 1, Exercise = exercise1 }
                    }
                }
            }.AsQueryable();

            _mockContext.Setup(c => c.ApplicationUsers).Returns(CreateMockDbSet(new List<ApplicationUser>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(workoutLogs).Object);
            _mockContext.Setup(c => c.Exercises).Returns(CreateMockDbSet(new List<Exercise> { exercise1, exercise2 }.AsQueryable()).Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.FoodItems).Returns(CreateMockDbSet(new List<FoodItem>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Goals).Returns(CreateMockDbSet(new List<Goal>().AsQueryable()).Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.WorkoutStatistics.TopExercises);
            Assert.Equal(2, result.WorkoutStatistics.TopExercises.Count);
            Assert.Equal("Bench Press", result.WorkoutStatistics.TopExercises[0].ExerciseName);
            Assert.Equal(3, result.WorkoutStatistics.TopExercises[0].UsageCount);
        }

        [Fact]
        public async Task Handle_NutritionStatistics_ReturnsTopFoods()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = 365 };

            var food1 = new FoodItem { FoodItemId = 1, Name = "Chicken Breast", CaloriesKcal = 165 };
            var food2 = new FoodItem { FoodItemId = 2, Name = "Rice", CaloriesKcal = 130 };

            var nutritionLogs = new List<NutritionLog>
            {
                new NutritionLog
                {
                    NutritionLogId = 1,
                    LogDate = DateTime.UtcNow.AddDays(-5),
                    FoodEntries = new List<FoodEntry>
                    {
                        new FoodEntry { FoodItemId = 1, FoodItem = food1, CaloriesKcal = 165 },
                        new FoodEntry { FoodItemId = 1, FoodItem = food1, CaloriesKcal = 165 },
                        new FoodEntry { FoodItemId = 2, FoodItem = food2, CaloriesKcal = 130 }
                    }
                }
            }.AsQueryable();

            _mockContext.Setup(c => c.ApplicationUsers).Returns(CreateMockDbSet(new List<ApplicationUser>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Exercises).Returns(CreateMockDbSet(new List<Exercise>().AsQueryable()).Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(nutritionLogs).Object);
            _mockContext.Setup(c => c.FoodItems).Returns(CreateMockDbSet(new List<FoodItem> { food1, food2 }.AsQueryable()).Object);
            _mockContext.Setup(c => c.Goals).Returns(CreateMockDbSet(new List<Goal>().AsQueryable()).Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.NutritionStatistics.TopFoods);
            Assert.Equal(2, result.NutritionStatistics.TopFoods.Count);
            Assert.Equal("Chicken Breast", result.NutritionStatistics.TopFoods[0].FoodName);
            Assert.Equal(2, result.NutritionStatistics.TopFoods[0].UsageCount);
        }

        [Fact]
        public async Task Handle_GoalStatistics_CalculatesCompletionRate()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = 365 };

            var goals = new List<Goal>
            {
                new Goal { GoalId = 1, Type = "Weight Loss", Status = "Completed" },
                new Goal { GoalId = 2, Type = "Weight Loss", Status = "InProgress" },
                new Goal { GoalId = 3, Type = "Muscle Gain", Status = "Active" },
                new Goal { GoalId = 4, Type = "Weight Loss", Status = "Completed" }
            }.AsQueryable();

            _mockContext.Setup(c => c.ApplicationUsers).Returns(CreateMockDbSet(new List<ApplicationUser>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Exercises).Returns(CreateMockDbSet(new List<Exercise>().AsQueryable()).Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.FoodItems).Returns(CreateMockDbSet(new List<FoodItem>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Goals).Returns(CreateMockDbSet(goals).Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(4, result.GoalStatistics.TotalGoals);
            Assert.Equal(2, result.GoalStatistics.ActiveGoals);
            Assert.Equal(2, result.GoalStatistics.CompletedGoals);
            Assert.Equal(50, result.GoalStatistics.GoalCompletionRate);
        }

        [Fact]
        public async Task Handle_DefaultsDays_To365WhenNotProvided()
        {
            // Arrange
            var query = new GetAdminStatisticsQuery { Days = null };
            SetupMockData();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            // Should use default 365 days without error
        }

        private void SetupMockData()
        {
            _mockContext.Setup(c => c.ApplicationUsers).Returns(CreateMockDbSet(new List<ApplicationUser>().AsQueryable()).Object);
            _mockContext.Setup(c => c.WorkoutLogs).Returns(CreateMockDbSet(new List<WorkoutLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Exercises).Returns(CreateMockDbSet(new List<Exercise>().AsQueryable()).Object);
            _mockContext.Setup(c => c.NutritionLogs).Returns(CreateMockDbSet(new List<NutritionLog>().AsQueryable()).Object);
            _mockContext.Setup(c => c.FoodItems).Returns(CreateMockDbSet(new List<FoodItem>().AsQueryable()).Object);
            _mockContext.Setup(c => c.Goals).Returns(CreateMockDbSet(new List<Goal>().AsQueryable()).Object);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            return data.BuildMockDbSet();
        }
    }
}

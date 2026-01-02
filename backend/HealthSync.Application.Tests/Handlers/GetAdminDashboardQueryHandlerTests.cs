using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Minio;
using Minio.DataModel.Result; // Fixed Namespace for MinIO v6+
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetAdminDashboardQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMinioClient> _minioMock;
    private readonly GetAdminDashboardQueryHandler _handler;

    public GetAdminDashboardQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _minioMock = new Mock<IMinioClient>();
        _handler = new GetAdminDashboardQueryHandler(_contextMock.Object, _minioMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCompleteKpiStats()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        // 1. Setup Users for KPI
        var users = new List<ApplicationUser>
        {
            new() { UserId = 111, Email = "user1@test.com", CreatedAt = lastMonth.AddDays(-10), IsActive = true }, // Existing
            new() { UserId = 222, Email = "user2@test.com", CreatedAt = now.AddDays(-2), IsActive = true },      // New
            new() { UserId = 333, Email = "user3@test.com", CreatedAt = now.AddDays(-1), IsActive = true }       // New
        };
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());

        // 2. Setup Active user activity
        var workoutLogs = new List<WorkoutLog>
        {
            new() { UserId = 222, WorkoutDate = now } // Active today
        };
        _contextMock.Setup(c => c.WorkoutLogs).Returns(workoutLogs.AsQueryable().BuildMock());

        var nutritionLogs = new List<NutritionLog>
        {
            new() { UserId = 333, LogDate = now } // Active today
        };
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());

        // 3. Setup Content Counts
        _contextMock.Setup(c => c.Exercises).Returns(new List<Exercise> { new() { ExerciseId = 1 }, new() { ExerciseId = 2 } }.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.FoodItems).Returns(new List<FoodItem> { new() { FoodItemId = 1 } }.AsQueryable().BuildMock());

        // 4. Setup AI Usage
        var chatMessages = new List<ChatMessage>
        {
            new() { Role = "model" }, new() { Role = "model" }, new() { Role = "user" }
        };
        _contextMock.Setup(c => c.ChatMessages).Returns(chatMessages.AsQueryable().BuildMock());

        // Mock empty sets to avoid NullReferences
        _contextMock.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.ExerciseSessions).Returns(new List<ExerciseSession>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.FoodEntries).Returns(new List<FoodEntry>().AsQueryable().BuildMock());

        // Setup MinIO - Using the correct return type
        _minioMock.Setup(m => m.ListBucketsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListAllMyBucketsResult());

        _contextMock.Setup(c => c.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(new GetAdminDashboardQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        
        // Validation of KPI Logic
        Assert.Equal(3, result.KpiStats.TotalUsers.Value);
        Assert.Equal(1, result.KpiStats.ActiveUsers.Daily);
        Assert.Equal(2, result.KpiStats.ContentCount.Exercises);
        Assert.Equal(1, result.KpiStats.ContentCount.FoodItems);
        Assert.Equal(2, result.KpiStats.AiUsage.TotalRequests);
    }

    [Fact]
    public async Task Handle_ShouldReturnSystemHealth_Offline_WhenMinioFails()
    {
        // Arrange
        // (Minimal DB Setup)
        _contextMock.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Exercises).Returns(new List<Exercise>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.FoodItems).Returns(new List<FoodItem>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.ChatMessages).Returns(new List<ChatMessage>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.ExerciseSessions).Returns(new List<ExerciseSession>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.FoodEntries).Returns(new List<FoodEntry>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        // Setup MinIO Exception
        _minioMock.Setup(m => m.ListBucketsAsync(It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception("Network Error"));

        // Act
        var result = await _handler.Handle(new GetAdminDashboardQuery(), CancellationToken.None);

        // Assert
        var minioService = result.SystemHealth.Services.FirstOrDefault(s => s.Name.Contains("MinIO"));
        Assert.NotNull(minioService);
        Assert.Equal("Offline", minioService.Status);
    }
}

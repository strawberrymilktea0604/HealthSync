using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetDashboardSummaryQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCounts()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new() { UserId = 1, Email = "user1@test.com", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { UserId = 2, Email = "user2@test.com", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = 3, Email = "user3@test.com", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };

        var workoutLogs = new List<WorkoutLog>
        {
            new() { WorkoutLogId = 1, UserId = 1 },
            new() { WorkoutLogId = 2, UserId = 2 }
        };

        var nutritionLogs = new List<NutritionLog>
        {
            new() { NutritionLogId = 1, UserId = 1 },
            new() { NutritionLogId = 2, UserId = 1 },
            new() { NutritionLogId = 3, UserId = 2 }
        };

        var goals = new List<Goal>
        {
            new() { GoalId = 1, UserId = 1, Status = "active" },
            new() { GoalId = 2, UserId = 2, Status = "completed" },
            new() { GoalId = 3, UserId = 3, Status = "active" }
        };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(workoutLogs.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(nutritionLogs.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(goals.AsQueryable().BuildMock());

        var query = new GetDashboardSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalUsers);
        Assert.Equal(2, result.NewUsersThisMonth);
        Assert.Equal(2, result.TotalWorkoutLogs);
        Assert.Equal(3, result.TotalNutritionLogs);
        Assert.Equal(3, result.TotalGoals);
        Assert.Equal(2, result.ActiveGoals);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroCounts_WhenNoData()
    {
        // Arrange
        _contextMock.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());

        var query = new GetDashboardSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.TotalUsers);
        Assert.Equal(0, result.NewUsersThisMonth);
        Assert.Equal(0, result.TotalWorkoutLogs);
        Assert.Equal(0, result.TotalNutritionLogs);
        Assert.Equal(0, result.TotalGoals);
        Assert.Equal(0, result.ActiveGoals);
    }

    [Fact]
    public async Task Handle_ShouldCountOnlyNewUsersFromCurrentMonth()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new() { UserId = 1, Email = "user1@test.com", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { UserId = 2, Email = "user2@test.com", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = 3, Email = "user3@test.com", CreatedAt = DateTime.UtcNow }
        };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());

        var query = new GetDashboardSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalUsers);
        Assert.Equal(2, result.NewUsersThisMonth); // Only users 2 and 3
    }

    [Fact]
    public async Task Handle_ShouldCountOnlyActiveGoals()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new() { GoalId = 1, UserId = 1, Status = "active" },
            new() { GoalId = 2, UserId = 2, Status = "completed" },
            new() { GoalId = 3, UserId = 3, Status = "active" },
            new() { GoalId = 4, UserId = 4, Status = "cancelled" }
        };

        _contextMock.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(new List<WorkoutLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(goals.AsQueryable().BuildMock());

        var query = new GetDashboardSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(4, result.TotalGoals);
        Assert.Equal(2, result.ActiveGoals); // Only goals 1 and 3
    }

    [Fact]
    public async Task Handle_ShouldHandleLargeDatasets()
    {
        // Arrange
        var users = Enumerable.Range(1, 100).Select(i => new ApplicationUser
        {
            UserId = i,
            Email = $"user{i}@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        var workoutLogs = Enumerable.Range(1, 500).Select(i => new WorkoutLog
        {
            WorkoutLogId = i,
            UserId = i % 100 + 1
        }).ToList();

        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.WorkoutLogs).Returns(workoutLogs.AsQueryable().BuildMock());
        _contextMock.Setup(c => c.NutritionLogs).Returns(new List<NutritionLog>().AsQueryable().BuildMock());
        _contextMock.Setup(c => c.Goals).Returns(new List<Goal>().AsQueryable().BuildMock());

        var query = new GetDashboardSummaryQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.TotalUsers);
        Assert.Equal(500, result.TotalWorkoutLogs);
    }
}

using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Domain.Constants;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new DashboardController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk_WithDashboardSummary()
    {
        // Arrange
        var expectedSummary = new DashboardSummaryDto
        {
            TotalUsers = 100,
            TotalGoals = 50,
            TotalWorkoutLogs = 200,
            TotalNutritionLogs = 300
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _controller.GetSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var summary = Assert.IsType<DashboardSummaryDto>(okResult.Value);
        Assert.Equal(expectedSummary.TotalUsers, summary.TotalUsers);
        Assert.Equal(expectedSummary.TotalGoals, summary.TotalGoals);
        Assert.Equal(expectedSummary.TotalWorkoutLogs, summary.TotalWorkoutLogs);
        Assert.Equal(expectedSummary.TotalNutritionLogs, summary.TotalNutritionLogs);
    }

    [Fact]
    public async Task GetCustomerDashboard_ShouldReturnOk_WithValidUserId()
    {
        // Arrange
        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedDashboard = new CustomerDashboardDto
        {
            UserInfo = new UserInfoDto { UserId = userId, Email = "test@example.com", FullName = "Test User" },
            GoalProgress = new GoalProgressDto { GoalType = "Weight", StartValue = 80, CurrentValue = 75, TargetValue = 70, Progress = 75, Remaining = 5, Status = "In Progress" },
            ActiveGoals = new List<GoalSummaryDto>(),
            WeightProgress = new WeightProgressDto { CurrentWeight = 75, TargetWeight = 70, WeightLost = 5, WeightRemaining = 5, ProgressPercentage = 50, WeightHistory = new List<WeightDataPointDto>(), DaysRemaining = 30, TimeRemaining = "30 days" },
            TodayStats = new TodayStatsDto { CaloriesConsumed = 1500, CaloriesTarget = 2000, WorkoutMinutes = 45, WorkoutDuration = "45 min" },
            ExerciseStreak = new ExerciseStreakDto { CurrentStreak = 5, TotalDays = 20 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCustomerDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDashboard);

        // Act
        var result = await _controller.GetCustomerDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dashboard = Assert.IsType<CustomerDashboardDto>(okResult.Value);
        Assert.Equal(expectedDashboard.UserInfo.UserId, dashboard.UserInfo.UserId);
        Assert.Equal(expectedDashboard.GoalProgress.GoalType, dashboard.GoalProgress.GoalType);
        Assert.Equal(expectedDashboard.WeightProgress.CurrentWeight, dashboard.WeightProgress.CurrentWeight);
        Assert.Equal(expectedDashboard.TodayStats.CaloriesConsumed, dashboard.TodayStats.CaloriesConsumed);
        Assert.Equal(expectedDashboard.ExerciseStreak.CurrentStreak, dashboard.ExerciseStreak.CurrentStreak);
    }

    [Fact]
    public async Task GetCustomerDashboard_ShouldReturnUnauthorized_WithMissingUserIdClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetCustomerDashboard();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var message = unauthorizedResult.Value!.GetType().GetProperty("message")?.GetValue(unauthorizedResult.Value);
        Assert.Equal("Invalid user token", message);
    }

    [Fact]
    public async Task GetCustomerDashboard_ShouldReturnUnauthorized_WithInvalidUserIdClaim()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-id")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetCustomerDashboard();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var message = unauthorizedResult.Value?.GetType().GetProperty("message")?.GetValue(unauthorizedResult.Value);
        Assert.Equal("Invalid user token", message);
    }
}
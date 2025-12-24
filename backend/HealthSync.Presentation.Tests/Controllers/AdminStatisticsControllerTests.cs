using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class AdminStatisticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AdminStatisticsController _controller;

    public AdminStatisticsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AdminStatisticsController(_mediatorMock.Object);
    }

    private AdminStatisticsDto CreateMockStatistics()
    {
        return new AdminStatisticsDto
        {
            UserStatistics = new UserStatisticsDto
            {
                TotalUsers = 100,
                ActiveUsers = 80,
                NewUsersThisMonth = 10,
                NewUsersThisWeek = 3,
                UserGrowthData = new List<UserGrowthDto>(),
                UserRoleDistribution = new List<UserRoleDistributionDto>()
            },
            WorkoutStatistics = new WorkoutStatisticsDto
            {
                TotalWorkoutLogs = 500,
                WorkoutLogsThisMonth = 50,
                TotalExercises = 100,
                TopExercises = new List<PopularExerciseDto>(),
                WorkoutActivityData = new List<WorkoutActivityDto>(),
                MuscleGroupDistribution = new List<MuscleGroupDistributionDto>()
            },
            NutritionStatistics = new NutritionStatisticsDto
            {
                TotalNutritionLogs = 300,
                NutritionLogsThisMonth = 30,
                TotalFoodItems = 200,
                TopFoods = new List<PopularFoodDto>(),
                NutritionActivityData = new List<NutritionActivityDto>(),
                AverageDailyNutrition = new AverageNutritionDto()
            },
            GoalStatistics = new GoalStatisticsDto
            {
                TotalGoals = 200,
                ActiveGoals = 150,
                CompletedGoals = 50,
                GoalCompletionRate = 0.25M,
                GoalTypeDistribution = new List<GoalTypeDistributionDto>(),
                GoalStatusDistribution = new List<GoalStatusDistributionDto>()
            }
        };
    }

    [Fact]
    public async Task GetStatistics_WithDefaultDays_ReturnsOkWithStatistics()
    {
        // Arrange
        var statistics = CreateMockStatistics();

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetAdminStatisticsQuery>(q => q.Days == 365),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetStatistics(365);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualStats = Assert.IsType<AdminStatisticsDto>(okResult.Value);
        Assert.Equal(100, actualStats.UserStatistics.TotalUsers);
    }

    [Fact]
    public async Task GetStatistics_WithCustomDays_ReturnsOkWithStatistics()
    {
        // Arrange
        var statistics = CreateMockStatistics();
        statistics.UserStatistics.TotalUsers = 50;

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetAdminStatisticsQuery>(q => q.Days == 30),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetStatistics(30);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualStats = Assert.IsType<AdminStatisticsDto>(okResult.Value);
        Assert.Equal(50, actualStats.UserStatistics.TotalUsers);
    }

    [Fact]
    public async Task GetUserStatistics_WithValidDays_ReturnsOkWithUserStats()
    {
        // Arrange
        var statistics = CreateMockStatistics();

        _mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAdminStatisticsQuery>(),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetUserStatistics(365);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var userStats = Assert.IsType<UserStatisticsDto>(okResult.Value);
        Assert.Equal(100, userStats.TotalUsers);
        Assert.Equal(80, userStats.ActiveUsers);
    }

    [Fact]
    public async Task GetWorkoutStatistics_WithValidDays_ReturnsOkWithWorkoutStats()
    {
        // Arrange
        var statistics = CreateMockStatistics();

        _mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAdminStatisticsQuery>(),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetWorkoutStatistics(365);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var workoutStats = Assert.IsType<WorkoutStatisticsDto>(okResult.Value);
        Assert.Equal(500, workoutStats.TotalWorkoutLogs);
        Assert.Equal(100, workoutStats.TotalExercises);
    }

    [Fact]
    public async Task GetNutritionStatistics_WithValidDays_ReturnsOkWithNutritionStats()
    {
        // Arrange
        var statistics = CreateMockStatistics();

        _mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAdminStatisticsQuery>(),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetNutritionStatistics(365);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var nutritionStats = Assert.IsType<NutritionStatisticsDto>(okResult.Value);
        Assert.Equal(300, nutritionStats.TotalNutritionLogs);
        Assert.Equal(200, nutritionStats.TotalFoodItems);
    }

    [Fact]
    public async Task GetGoalStatistics_WithValidDays_ReturnsOkWithGoalStats()
    {
        // Arrange
        var statistics = CreateMockStatistics();

        _mediatorMock.Setup(m => m.Send(
            It.IsAny<GetAdminStatisticsQuery>(),
            default))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetGoalStatistics(365);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var goalStats = Assert.IsType<GoalStatisticsDto>(okResult.Value);
        Assert.Equal(200, goalStats.TotalGoals);
        Assert.Equal(50, goalStats.CompletedGoals);
        Assert.Equal(150, goalStats.ActiveGoals);
    }

}

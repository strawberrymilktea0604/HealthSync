using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class GoalsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly GoalsController _controller;

    public GoalsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new GoalsController(_mediatorMock.Object);
    }

    private void SetupUserClaims(int userId)
    {
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
    }

    [Fact]
    public async Task CreateGoal_ShouldReturnCreatedAtAction_WithValidData()
    {
        // Arrange
        SetupUserClaims(1);
        var request = new CreateGoalRequest
        {
            Type = "WeightLoss",
            TargetValue = 10.0M,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Notes = "Test goal"
        };

        var expectedResult = new GoalResponse
        {
            GoalId = 1,
            Type = "WeightLoss",
            TargetValue = 10.0M,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Active",
            Notes = "Test goal"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateGoalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateGoal(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(GoalsController.GetGoals), createdResult.ActionName);
        var goal = Assert.IsType<GoalResponse>(createdResult.Value);
        Assert.Equal(expectedResult.GoalId, goal.GoalId);
        Assert.Equal(expectedResult.Type, goal.Type);
        Assert.Equal(expectedResult.TargetValue, goal.TargetValue);
    }

    [Fact]
    public async Task CreateGoal_ShouldReturnUnauthorized_WithoutUserId()
    {
        // Arrange
        var request = new CreateGoalRequest
        {
            Type = "WeightLoss",
            TargetValue = 10.0M,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1)
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.CreateGoal(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetGoals_ShouldReturnOk_WithGoalsList()
    {
        // Arrange
        SetupUserClaims(1);
        var expectedResponse = new GetGoalsResponse
        {
            Goals = new List<GoalResponse>
            {
                new GoalResponse { GoalId = 1, Type = "WeightLoss", TargetValue = 10.0M, StartDate = DateTime.Now, Status = "Active" },
                new GoalResponse { GoalId = 2, Type = "MuscleGain", TargetValue = 5.0M, StartDate = DateTime.Now, Status = "Active" }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoalsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetGoals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetGoalsResponse>(okResult.Value);
        Assert.Equal(2, response.Goals.Count);
        Assert.Equal(expectedResponse.Goals[0].GoalId, response.Goals[0].GoalId);
        Assert.Equal(expectedResponse.Goals[1].GoalId, response.Goals[1].GoalId);
    }

    [Fact]
    public async Task GetGoals_ShouldReturnUnauthorized_WithoutUserId()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetGoals();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AddProgress_ShouldReturnOk_WithValidData()
    {
        // Arrange
        SetupUserClaims(1);
        var goalId = 1;
        var request = new AddProgressRequest
        {
            RecordDate = DateTime.Now,
            Value = 2.5M,
            Notes = "Good progress",
            WeightKg = 70.0M,
            WaistCm = 85.0M
        };

        var expectedResult = new AddProgressResponse
        {
            ProgressRecord = new ProgressRecordResponse
            {
                ProgressRecordId = 1,
                RecordDate = request.RecordDate,
                Value = request.Value,
                Notes = request.Notes,
                WeightKg = request.WeightKg.Value,
                WaistCm = request.WaistCm.Value
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<AddProgressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.AddProgress(goalId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var progress = Assert.IsType<AddProgressResponse>(okResult.Value);
        Assert.Equal(expectedResult.ProgressRecord.ProgressRecordId, progress.ProgressRecord.ProgressRecordId);
        Assert.Equal(expectedResult.ProgressRecord.Value, progress.ProgressRecord.Value);
    }

    [Fact]
    public async Task AddProgress_ShouldReturnNotFound_WhenGoalNotFound()
    {
        // Arrange
        SetupUserClaims(1);
        var goalId = 999;
        var request = new AddProgressRequest
        {
            RecordDate = DateTime.Now,
            Value = 2.5M
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<AddProgressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Goal not found"));

        // Act
        var result = await _controller.AddProgress(goalId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Goal not found", notFoundResult.Value);
    }

    [Fact]
    public async Task AddProgress_ShouldReturnUnauthorized_WithoutUserId()
    {
        // Arrange
        var goalId = 1;
        var request = new AddProgressRequest
        {
            RecordDate = DateTime.Now,
            Value = 2.5M
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.AddProgress(goalId, request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
using System.Security.Claims;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class WorkoutControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly WorkoutController _controller;

    public WorkoutControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new WorkoutController(_mediatorMock.Object);
    }

    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("uid", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetExercises_WithoutFilters_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto { ExerciseId = 1, Name = "Push-up", MuscleGroup = "Chest" },
            new ExerciseDto { ExerciseId = 2, Name = "Pull-up", MuscleGroup = "Back" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetExercisesQuery>(), default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Equal(2, returnedExercises.Count());
    }

    [Fact]
    public async Task GetExercises_WithMuscleGroupFilter_ReturnsFilteredExercises()
    {
        // Arrange
        var muscleGroup = "Chest";
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto { ExerciseId = 1, Name = "Push-up", MuscleGroup = "Chest" }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExercisesQuery>(q => q.MuscleGroup == muscleGroup),
            default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(muscleGroup, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Single(returnedExercises);
        Assert.Equal("Chest", returnedExercises.First().MuscleGroup);
    }

    [Fact]
    public async Task GetExercises_WithDifficultyAndSearch_ReturnsFilteredExercises()
    {
        // Arrange
        var difficulty = "Beginner";
        var search = "push";
        var exercises = new List<ExerciseDto>
        {
            new ExerciseDto 
            { 
                ExerciseId = 1, 
                Name = "Push-up", 
                MuscleGroup = "Chest",
                Difficulty = "Beginner"
            }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetExercisesQuery>(q => q.Difficulty == difficulty && q.Search == search),
            default))
            .ReturnsAsync(exercises);

        // Act
        var result = await _controller.GetExercises(null, difficulty, search);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedExercises = Assert.IsAssignableFrom<IEnumerable<ExerciseDto>>(okResult.Value);
        Assert.Single(returnedExercises);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithValidUser_ReturnsWorkoutLogs()
    {
        // Arrange
        var userId = 1;
        SetupUserClaims(userId);

        var workoutLogs = new List<WorkoutLogDto>
        {
            new WorkoutLogDto 
            { 
                WorkoutLogId = 1, 
                UserId = userId, 
                WorkoutDate = DateTime.UtcNow.AddDays(-1)
            },
            new WorkoutLogDto 
            { 
                WorkoutLogId = 2, 
                UserId = userId, 
                WorkoutDate = DateTime.UtcNow
            }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetWorkoutLogsQuery>(q => q.UserId == userId),
            default))
            .ReturnsAsync(workoutLogs);

        // Act
        var result = await _controller.GetWorkoutLogs(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<WorkoutLogDto>>(okResult.Value);
        Assert.Equal(2, returnedLogs.Count());
    }

    [Fact]
    public async Task GetWorkoutLogs_WithDateRange_ReturnsFilteredLogs()
    {
        // Arrange
        var userId = 1;
        SetupUserClaims(userId);

        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workoutLogs = new List<WorkoutLogDto>
        {
            new WorkoutLogDto 
            { 
                WorkoutLogId = 1, 
                UserId = userId, 
                WorkoutDate = DateTime.UtcNow.AddDays(-3)
            }
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetWorkoutLogsQuery>(q => 
                q.UserId == userId && 
                q.StartDate == startDate && 
                q.EndDate == endDate),
            default))
            .ReturnsAsync(workoutLogs);

        // Act
        var result = await _controller.GetWorkoutLogs(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<WorkoutLogDto>>(okResult.Value);
        Assert.Single(returnedLogs);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetWorkoutLogs(null, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid_id") // Not a valid integer
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetWorkoutLogs(null, null);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var userId = 1;
        SetupUserClaims(userId);

        var dto = new CreateWorkoutLogDto
        {
            WorkoutDate = DateTime.UtcNow,
            DurationMin = 60,
            ExerciseSessions = new List<CreateExerciseSessionDto>
            {
                new CreateExerciseSessionDto
                {
                    ExerciseId = 1,
                    Sets = 3,
                    Reps = 10,
                    WeightKg = 50
                }
            }
        };

        var workoutLogId = 10;
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateWorkoutLogCommand>(c => c.UserId == userId),
            default))
            .ReturnsAsync(workoutLogId);

        // Act
        var result = await _controller.CreateWorkoutLog(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(WorkoutController.GetWorkoutLogs), createdResult.ActionName);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(workoutLogId, actualId);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange - Set up empty user with no claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var dto = new CreateWorkoutLogDto
        {
            WorkoutDate = DateTime.UtcNow,
            ExerciseSessions = new List<CreateExerciseSessionDto>()
        };

        // Act
        var result = await _controller.CreateWorkoutLog(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not_a_number")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var dto = new CreateWorkoutLogDto
        {
            WorkoutDate = DateTime.UtcNow,
            ExerciseSessions = new List<CreateExerciseSessionDto>()
        };

        // Act
        var result = await _controller.CreateWorkoutLog(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithMultipleExercises_CreatesSuccessfully()
    {
        // Arrange
        var userId = 2;
        SetupUserClaims(userId);

        var dto = new CreateWorkoutLogDto
        {
            WorkoutDate = DateTime.UtcNow,
            DurationMin = 90,
            ExerciseSessions = new List<CreateExerciseSessionDto>
            {
                new CreateExerciseSessionDto
                {
                    ExerciseId = 1,
                    Sets = 3,
                    Reps = 10,
                    WeightKg = 50
                },
                new CreateExerciseSessionDto
                {
                    ExerciseId = 2,
                    Sets = 4,
                    Reps = 8,
                    WeightKg = 60
                }
            }
        };

        var workoutLogId = 20;
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateWorkoutLogCommand>(c => 
                c.UserId == userId && 
                c.WorkoutLog.ExerciseSessions.Count == 2),
            default))
            .ReturnsAsync(workoutLogId);

        // Act
        var result = await _controller.CreateWorkoutLog(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var routeValues = (Microsoft.AspNetCore.Routing.RouteValueDictionary)createdResult.RouteValues!;
        var actualId = (int)routeValues["id"]!;
        Assert.Equal(workoutLogId, actualId);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateWorkoutLogCommand>(c => c.WorkoutLog.ExerciseSessions.Count == 2),
            default), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutLogs_ReturnsEmptyList_WhenNoLogsExist()
    {
        // Arrange
        var userId = 3;
        SetupUserClaims(userId);

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetWorkoutLogsQuery>(q => q.UserId == userId),
            default))
            .ReturnsAsync(new List<WorkoutLogDto>());

        // Act
        var result = await _controller.GetWorkoutLogs(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<WorkoutLogDto>>(okResult.Value);
        Assert.Empty(returnedLogs);
    }
}

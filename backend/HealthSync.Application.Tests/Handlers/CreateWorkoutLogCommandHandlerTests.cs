using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateWorkoutLogCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateWorkoutLogCommandHandler _handler;

    public CreateWorkoutLogCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateWorkoutLogCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidWorkoutLog_ReturnsWorkoutLogId()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.UtcNow,
                DurationMin = 60,
                Notes = "Test workout",
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
            }
        };

        var capturedWorkoutLog = null as WorkoutLog;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(wl => capturedWorkoutLog = wl);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { if (capturedWorkoutLog != null) capturedWorkoutLog.WorkoutLogId = 123; })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(123, result);
        _mockContext.Verify(x => x.Add(It.IsAny<WorkoutLog>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidWorkoutLog_CreatesCorrectExerciseSessions()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.UtcNow,
                DurationMin = 45,
                Notes = "Chest day",
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 4,
                        Reps = 12,
                        WeightKg = 60,
                        RestSec = 90,
                        Rpe = 8
                    },
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 2,
                        Sets = 3,
                        Reps = 15,
                        WeightKg = 40
                    }
                }
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(wl => capturedWorkoutLog = wl);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        Assert.Equal(2, capturedWorkoutLog.ExerciseSessions.Count);
        Assert.Equal(1, capturedWorkoutLog.ExerciseSessions.First().ExerciseId);
        Assert.Equal(4, capturedWorkoutLog.ExerciseSessions.First().Sets);
        Assert.Equal(12, capturedWorkoutLog.ExerciseSessions.First().Reps);
    }

    [Fact]
    public async Task Handle_EmptyExerciseSessions_CreatesWorkoutLogWithNoSessions()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.UtcNow,
                DurationMin = 30,
                Notes = "Rest day walk",
                ExerciseSessions = new List<CreateExerciseSessionDto>()
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(wl => capturedWorkoutLog = wl);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        Assert.Empty(capturedWorkoutLog.ExerciseSessions);
    }
}

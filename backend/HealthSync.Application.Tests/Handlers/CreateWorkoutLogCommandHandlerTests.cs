using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;

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
    public async Task Handle_ValidCommand_CreatesWorkoutLogSuccessfully()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                Notes = "Good workout",
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50,
                        RestSec = 60,
                        Rpe = 7
                    }
                }
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => capturedWorkoutLog = w);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (capturedWorkoutLog != null)
                {
                    capturedWorkoutLog.WorkoutLogId = 123; // Simulate DB-generated ID
                }
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(123, result);
        Assert.NotNull(capturedWorkoutLog);
        Assert.Equal(1, capturedWorkoutLog.UserId);
        Assert.Equal(60, capturedWorkoutLog.DurationMin);
        Assert.Equal("Good workout", capturedWorkoutLog.Notes);
        Assert.Single(capturedWorkoutLog.ExerciseSessions);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsContextAddOnce()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 45,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 12,
                        WeightKg = 40
                    }
                }
            }
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.Add(It.IsAny<WorkoutLog>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsync()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 30,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 2,
                        Sets = 4,
                        Reps = 8,
                        WeightKg = 60
                    }
                }
            }
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleExerciseSessions_SavesAllCorrectly()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
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
                        Reps = 12,
                        WeightKg = 30
                    },
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 3,
                        Sets = 2,
                        Reps = 15,
                        WeightKg = 20
                    }
                }
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => capturedWorkoutLog = w);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        Assert.Equal(3, capturedWorkoutLog.ExerciseSessions.Count);
        
        var session1 = capturedWorkoutLog.ExerciseSessions.First(s => s.ExerciseId == 1);
        Assert.Equal(3, session1.Sets);
        Assert.Equal(10, session1.Reps);
        Assert.Equal(50, session1.WeightKg);

        var session2 = capturedWorkoutLog.ExerciseSessions.First(s => s.ExerciseId == 2);
        Assert.Equal(4, session2.Sets);
        Assert.Equal(12, session2.Reps);
        Assert.Equal(30, session2.WeightKg);
    }

    [Fact]
    public async Task Handle_WorkoutLogWithNotes_SavesNotesCorrectly()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                Notes = "Felt strong today, increased weight on squats",
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 5,
                        Reps = 5,
                        WeightKg = 100
                    }
                }
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => capturedWorkoutLog = w);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        Assert.Equal("Felt strong today, increased weight on squats", capturedWorkoutLog.Notes);
    }

    [Fact]
    public async Task Handle_ExerciseSessionWithOptionalFields_SavesCorrectly()
    {
        // Arrange
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = DateTime.Now,
                DurationMin = 45,
                ExerciseSessions = new List<CreateExerciseSessionDto>
                {
                    new CreateExerciseSessionDto
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50,
                        RestSec = 90,
                        Rpe = 8
                    }
                }
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => capturedWorkoutLog = w);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        var session = capturedWorkoutLog.ExerciseSessions.First();
        Assert.Equal(90, session.RestSec);
        Assert.Equal(8, session.Rpe);
    }

    [Fact]
    public async Task Handle_WorkoutDatePreserved_SavesCorrectDate()
    {
        // Arrange
        var specificDate = new DateTime(2024, 12, 15, 10, 30, 0);
        var command = new CreateWorkoutLogCommand
        {
            UserId = 1,
            WorkoutLog = new CreateWorkoutLogDto
            {
                WorkoutDate = specificDate,
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
            }
        };

        WorkoutLog? capturedWorkoutLog = null;
        _mockContext.Setup(x => x.Add(It.IsAny<WorkoutLog>()))
            .Callback<WorkoutLog>(w => capturedWorkoutLog = w);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWorkoutLog);
        Assert.Equal(specificDate, capturedWorkoutLog.WorkoutDate);
    }
}

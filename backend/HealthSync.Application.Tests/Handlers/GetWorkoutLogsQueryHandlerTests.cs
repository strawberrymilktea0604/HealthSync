using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;

namespace HealthSync.Application.Tests.Handlers;

public class GetWorkoutLogsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetWorkoutLogsQueryHandler _handler;

    public GetWorkoutLogsQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetWorkoutLogsQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_UserHasWorkoutLogs_ReturnsAllLogs()
    {
        // Arrange
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = DateTime.Now.AddDays(-1),
                DurationMin = 60,
                Notes = "Good workout",
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseSessionId = 1,
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50,
                        Exercise = new Exercise { ExerciseId = 1, Name = "Bench Press" }
                    }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 2,
                UserId = 1,
                WorkoutDate = DateTime.Now.AddDays(-2),
                DurationMin = 45,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseSessionId = 2,
                        ExerciseId = 2,
                        Sets = 4,
                        Reps = 12,
                        WeightKg = 40,
                        Exercise = new Exercise { ExerciseId = 2, Name = "Squat" }
                    }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, log => Assert.Equal(1, log.UserId));
    }

    [Fact]
    public async Task Handle_UserHasNoLogs_ReturnsEmptyList()
    {
        // Arrange
        var workoutLogs = new List<WorkoutLog>();

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery { UserId = 999 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FilterByStartDate_ReturnsCorrectLogs()
    {
        // Arrange
        var startDate = new DateTime(2024, 12, 15);
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 20),
                DurationMin = 60,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        Exercise = new Exercise { Name = "Push-ups" }
                    }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 2,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 10), // Before start date
                DurationMin = 45,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseId = 2,
                        Sets = 4,
                        Reps = 12,
                        Exercise = new Exercise { Name = "Squats" }
                    }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery
        {
            UserId = 1,
            StartDate = startDate
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].WorkoutLogId);
        Assert.True(result[0].WorkoutDate >= startDate);
    }

    [Fact]
    public async Task Handle_FilterByEndDate_ReturnsCorrectLogs()
    {
        // Arrange
        var endDate = new DateTime(2024, 12, 15);
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 10),
                DurationMin = 60,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 1, Sets = 3, Reps = 10, Exercise = new Exercise { Name = "Test" } }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 2,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 20), // After end date
                DurationMin = 45,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 2, Sets = 4, Reps = 12, Exercise = new Exercise { Name = "Test2" } }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery
        {
            UserId = 1,
            EndDate = endDate
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].WorkoutLogId);
        Assert.True(result[0].WorkoutDate <= endDate);
    }

    [Fact]
    public async Task Handle_FilterByDateRange_ReturnsCorrectLogs()
    {
        // Arrange
        var startDate = new DateTime(2024, 12, 10);
        var endDate = new DateTime(2024, 12, 20);
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 15), // Within range
                DurationMin = 60,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 1, Sets = 3, Reps = 10, Exercise = new Exercise { Name = "Test" } }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 2,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 5), // Before range
                DurationMin = 45,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 2, Sets = 4, Reps = 12, Exercise = new Exercise { Name = "Test2" } }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 3,
                UserId = 1,
                WorkoutDate = new DateTime(2024, 12, 25), // After range
                DurationMin = 30,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 3, Sets = 2, Reps = 15, Exercise = new Exercise { Name = "Test3" } }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery
        {
            UserId = 1,
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].WorkoutLogId);
    }

    [Fact]
    public async Task Handle_IncludesExerciseSessions_MapsCorrectly()
    {
        // Arrange
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = DateTime.Now,
                DurationMin = 90,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession
                    {
                        ExerciseSessionId = 1,
                        ExerciseId = 1,
                        Sets = 3,
                        Reps = 10,
                        WeightKg = 50,
                        RestSec = 60,
                        Rpe = 7,
                        Exercise = new Exercise { ExerciseId = 1, Name = "Deadlift" }
                    },
                    new ExerciseSession
                    {
                        ExerciseSessionId = 2,
                        ExerciseId = 2,
                        Sets = 4,
                        Reps = 12,
                        WeightKg = 30,
                        RestSec = 45,
                        Rpe = 6,
                        Exercise = new Exercise { ExerciseId = 2, Name = "Pull-ups" }
                    }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].ExerciseSessions.Count);
        
        var session1 = result[0].ExerciseSessions.First(s => s.ExerciseId == 1);
        Assert.Equal("Deadlift", session1.ExerciseName);
        Assert.Equal(3, session1.Sets);
        Assert.Equal(10, session1.Reps);
        Assert.Equal(50, session1.WeightKg);

        var session2 = result[0].ExerciseSessions.First(s => s.ExerciseId == 2);
        Assert.Equal("Pull-ups", session2.ExerciseName);
        Assert.Equal(4, session2.Sets);
        Assert.Equal(12, session2.Reps);
    }

    [Fact]
    public async Task Handle_OnlyReturnsLogsForSpecifiedUser()
    {
        // Arrange
        var workoutLogs = new List<WorkoutLog>
        {
            new WorkoutLog
            {
                WorkoutLogId = 1,
                UserId = 1,
                WorkoutDate = DateTime.Now,
                DurationMin = 60,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 1, Sets = 3, Reps = 10, Exercise = new Exercise { Name = "Test" } }
                }
            },
            new WorkoutLog
            {
                WorkoutLogId = 2,
                UserId = 2, // Different user
                WorkoutDate = DateTime.Now,
                DurationMin = 45,
                ExerciseSessions = new List<ExerciseSession>
                {
                    new ExerciseSession { ExerciseId = 2, Sets = 4, Reps = 12, Exercise = new Exercise { Name = "Test2" } }
                }
            }
        };

        var mockWorkoutLogs = workoutLogs.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.WorkoutLogs).Returns(mockWorkoutLogs);

        var query = new GetWorkoutLogsQuery { UserId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, log => Assert.Equal(1, log.UserId));
    }
}

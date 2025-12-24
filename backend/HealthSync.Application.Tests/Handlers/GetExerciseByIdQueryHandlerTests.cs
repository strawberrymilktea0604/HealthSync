using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetExerciseByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetExerciseByIdQueryHandler _handler;

    public GetExerciseByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetExerciseByIdQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise
            {
                ExerciseId = 1,
                Name = "Bench Press",
                MuscleGroup = "Chest",
                Difficulty = "Intermediate",
                Equipment = "Barbell",
                Description = "Classic chest exercise"
            }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExerciseByIdQuery { ExerciseId = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.ExerciseId);
        Assert.Equal("Bench Press", result.Name);
        Assert.Equal("Chest", result.MuscleGroup);
        Assert.Equal("Intermediate", result.Difficulty);
        Assert.Equal("Barbell", result.Equipment);
        Assert.Equal("Classic chest exercise", result.Description);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Squat", MuscleGroup = "Legs" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExerciseByIdQuery { ExerciseId = 999 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnExerciseWithAllFields_WhenAllFieldsPresent()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise
            {
                ExerciseId = 5,
                Name = "Deadlift",
                MuscleGroup = "Back",
                Difficulty = "Advanced",
                Equipment = "Barbell",
                Description = "Full body compound movement"
            }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExerciseByIdQuery { ExerciseId = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Deadlift", result!.Name);
        Assert.Equal("Back", result.MuscleGroup);
        Assert.Equal("Advanced", result.Difficulty);
        Assert.Equal("Barbell", result.Equipment);
        Assert.Equal("Full body compound movement", result.Description);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectExercise_WhenMultipleExercisesExist()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Push-ups", MuscleGroup = "Chest" },
            new Exercise { ExerciseId = 2, Name = "Pull-ups", MuscleGroup = "Back" },
            new Exercise { ExerciseId = 3, Name = "Squats", MuscleGroup = "Legs" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExerciseByIdQuery { ExerciseId = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.ExerciseId);
        Assert.Equal("Pull-ups", result.Name);
        Assert.Equal("Back", result.MuscleGroup);
    }
}

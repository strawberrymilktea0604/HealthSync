using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetExercisesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetExercisesQueryHandler _handler;

    public GetExercisesQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetExercisesQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllExercises_WhenNoFiltersApplied()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest", Difficulty = "Intermediate" },
            new Exercise { ExerciseId = 2, Name = "Squat", MuscleGroup = "Legs", Difficulty = "Advanced" },
            new Exercise { ExerciseId = 3, Name = "Pull-ups", MuscleGroup = "Back", Difficulty = "Intermediate" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldFilterByMuscleGroup_WhenMuscleGroupProvided()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest", Difficulty = "Intermediate" },
            new Exercise { ExerciseId = 2, Name = "Push-ups", MuscleGroup = "Chest", Difficulty = "Beginner" },
            new Exercise { ExerciseId = 3, Name = "Squat", MuscleGroup = "Legs", Difficulty = "Advanced" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery { MuscleGroup = "Chest" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("Chest", e.MuscleGroup));
    }

    [Fact]
    public async Task Handle_ShouldFilterByDifficulty_WhenDifficultyProvided()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Push-ups", MuscleGroup = "Chest", Difficulty = "Beginner" },
            new Exercise { ExerciseId = 2, Name = "Bench Press", MuscleGroup = "Chest", Difficulty = "Intermediate" },
            new Exercise { ExerciseId = 3, Name = "Deadlift", MuscleGroup = "Back", Difficulty = "Advanced" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery { Difficulty = "Advanced" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Advanced", result[0].Difficulty);
        Assert.Equal("Deadlift", result[0].Name);
    }

    [Fact]
    public async Task Handle_ShouldSearchByName_WhenSearchTermProvided()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest", Description = "Classic chest exercise" },
            new Exercise { ExerciseId = 2, Name = "Dumbbell Press", MuscleGroup = "Chest", Description = "Variation with dumbbells" },
            new Exercise { ExerciseId = 3, Name = "Squat", MuscleGroup = "Legs", Description = "Leg exercise" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery { Search = "Press" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Contains("Press", e.Name));
    }

    [Fact]
    public async Task Handle_ShouldSearchByDescription_WhenSearchTermInDescription()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest", Description = "Classic exercise" },
            new Exercise { ExerciseId = 2, Name = "Squat", MuscleGroup = "Legs", Description = "Compound exercise" },
            new Exercise { ExerciseId = 3, Name = "Pull-ups", MuscleGroup = "Back", Description = "Upper body strength" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery { Search = "exercise" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldCombineFilters_WhenMultipleFiltersProvided()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Push-ups", MuscleGroup = "Chest", Difficulty = "Beginner" },
            new Exercise { ExerciseId = 2, Name = "Bench Press", MuscleGroup = "Chest", Difficulty = "Intermediate" },
            new Exercise { ExerciseId = 3, Name = "Dips", MuscleGroup = "Chest", Difficulty = "Intermediate" },
            new Exercise { ExerciseId = 4, Name = "Squat", MuscleGroup = "Legs", Difficulty = "Intermediate" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery
        {
            MuscleGroup = "Chest",
            Difficulty = "Intermediate"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e =>
        {
            Assert.Equal("Chest", e.MuscleGroup);
            Assert.Equal("Intermediate", e.Difficulty);
        });
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoExercisesMatchFilters()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { ExerciseId = 1, Name = "Bench Press", MuscleGroup = "Chest", Difficulty = "Intermediate" }
        };

        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery { MuscleGroup = "Arms" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoExercisesExist()
    {
        // Arrange
        var exercises = new List<Exercise>();
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var query = new GetExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

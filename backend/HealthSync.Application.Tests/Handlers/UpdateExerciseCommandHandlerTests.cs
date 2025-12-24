using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateExerciseCommandHandler _handler;

    public UpdateExerciseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateExerciseCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 1,
            Name = "Old Name",
            MuscleGroup = "Old Group",
            Difficulty = "Easy",
            Equipment = "None",
            Description = "Old description"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 1,
            Name = "Updated Name",
            MuscleGroup = "Chest",
            Difficulty = "Intermediate",
            Equipment = "Barbell",
            Description = "Updated description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Name", exercise.Name);
        Assert.Equal("Chest", exercise.MuscleGroup);
        Assert.Equal("Intermediate", exercise.Difficulty);
        Assert.Equal("Barbell", exercise.Equipment);
        Assert.Equal("Updated description", exercise.Description);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exercises = new List<Exercise>();
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 999,
            Name = "New Name",
            MuscleGroup = "Chest"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenAllFieldsProvided()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 2,
            Name = "Push-ups",
            MuscleGroup = "Chest",
            Difficulty = "Beginner",
            Equipment = "None",
            Description = "Basic exercise"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 2,
            Name = "Weighted Push-ups",
            MuscleGroup = "Chest",
            Difficulty = "Advanced",
            Equipment = "Weight Vest",
            Description = "Advanced variation with added weight"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Weighted Push-ups", exercise.Name);
        Assert.Equal("Advanced", exercise.Difficulty);
        Assert.Equal("Weight Vest", exercise.Equipment);
        Assert.Equal("Advanced variation with added weight", exercise.Description);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyTargetExercise_WhenMultipleExercisesExist()
    {
        // Arrange
        var targetExercise = new Exercise
        {
            ExerciseId = 1,
            Name = "Squat",
            MuscleGroup = "Legs",
            Difficulty = "Intermediate"
        };

        var otherExercise = new Exercise
        {
            ExerciseId = 2,
            Name = "Deadlift",
            MuscleGroup = "Back",
            Difficulty = "Advanced"
        };

        var exercises = new List<Exercise> { targetExercise, otherExercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 1,
            Name = "Front Squat",
            MuscleGroup = "Legs",
            Difficulty = "Advanced"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Front Squat", targetExercise.Name);
        Assert.Equal("Advanced", targetExercise.Difficulty);
        // Other exercise should remain unchanged
        Assert.Equal("Deadlift", otherExercise.Name);
        Assert.Equal("Back", otherExercise.MuscleGroup);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_OnlyWhenExerciseFound()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 3,
            Name = "Pull-ups",
            MuscleGroup = "Back"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 3,
            Name = "Chin-ups",
            MuscleGroup = "Back"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateEmptyDescription_WhenDescriptionProvided()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 5,
            Name = "Plank",
            MuscleGroup = "Core",
            Difficulty = "Beginner",
            Equipment = "None",
            Description = string.Empty
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new UpdateExerciseCommand
        {
            ExerciseId = 5,
            Name = "Plank",
            MuscleGroup = "Core",
            Difficulty = "Beginner",
            Equipment = "None",
            Description = "Hold body in straight line position"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Hold body in straight line position", exercise.Description);
    }
}

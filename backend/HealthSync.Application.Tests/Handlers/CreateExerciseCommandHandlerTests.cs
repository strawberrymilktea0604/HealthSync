using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateExerciseCommandHandler _handler;

    public CreateExerciseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateExerciseCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateExercise_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Bench Press",
            MuscleGroup = "Chest",
            Difficulty = "Intermediate",
            Equipment = "Barbell",
            Description = "Classic chest exercise"
        };

        Exercise? capturedExercise = null;
        _contextMock.Setup(c => c.Add(It.IsAny<Exercise>()))
            .Callback<object>(e => capturedExercise = e as Exercise);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Add(It.IsAny<Exercise>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.NotNull(capturedExercise);
        Assert.Equal("Bench Press", capturedExercise!.Name);
        Assert.Equal("Chest", capturedExercise.MuscleGroup);
        Assert.Equal("Intermediate", capturedExercise.Difficulty);
        Assert.Equal("Barbell", capturedExercise.Equipment);
        Assert.Equal("Classic chest exercise", capturedExercise.Description);
    }

    [Fact]
    public async Task Handle_ShouldReturnExerciseId_AfterCreation()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Squat",
            MuscleGroup = "Legs",
            Difficulty = "Advanced",
            Equipment = "Barbell",
            Description = "Compound leg exercise"
        };

        _contextMock.Setup(c => c.Add(It.IsAny<Exercise>()))
            .Callback<object>(e =>
            {
                if (e is Exercise exercise)
                {
                    exercise.ExerciseId = 5;
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task Handle_ShouldCreateExerciseWithAllFields_WhenAllFieldsProvided()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Deadlift",
            MuscleGroup = "Back",
            Difficulty = "Advanced",
            Equipment = "Barbell",
            Description = "Full body compound movement"
        };

        Exercise? capturedExercise = null;
        _contextMock.Setup(c => c.Add(It.IsAny<Exercise>()))
            .Callback<object>(e => capturedExercise = e as Exercise);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedExercise);
        Assert.Equal("Deadlift", capturedExercise!.Name);
        Assert.Equal("Back", capturedExercise.MuscleGroup);
        Assert.Equal("Advanced", capturedExercise.Difficulty);
        Assert.Equal("Barbell", capturedExercise.Equipment);
        Assert.Equal("Full body compound movement", capturedExercise.Description);
    }

    [Fact]
    public async Task Handle_ShouldCreateExerciseWithMinimalFields_WhenOptionalFieldsOmitted()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Push-ups",
            MuscleGroup = "Chest",
            Difficulty = "Beginner",
            Equipment = "None"
        };

        Exercise? capturedExercise = null;
        _contextMock.Setup(c => c.Add(It.IsAny<Exercise>()))
            .Callback<object>(e => capturedExercise = e as Exercise);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedExercise);
        Assert.Equal("Push-ups", capturedExercise!.Name);
        Assert.Equal("Chest", capturedExercise.MuscleGroup);
        Assert.Equal("Beginner", capturedExercise.Difficulty);
        Assert.Equal("None", capturedExercise.Equipment);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAsync_ExactlyOnce()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Pull-ups",
            MuscleGroup = "Back",
            Difficulty = "Intermediate",
            Equipment = "Pull-up Bar"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

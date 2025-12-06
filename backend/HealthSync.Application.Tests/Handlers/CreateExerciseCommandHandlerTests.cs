using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateExerciseCommandHandler _handler;

    public CreateExerciseCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateExerciseCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidExercise_ReturnsExerciseId()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Bench Press",
            MuscleGroup = "Chest",
            Difficulty = "Intermediate",
            Equipment = "Barbell",
            Description = "Compound chest exercise"
        };

        Exercise? capturedExercise = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { if (capturedExercise != null) capturedExercise.ExerciseId = 789; })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(789, result);
        _mockContext.Verify(x => x.Add(It.IsAny<Exercise>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidExercise_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Deadlift",
            MuscleGroup = "Back",
            Difficulty = "Advanced",
            Equipment = "Barbell",
            Description = "Compound back and leg exercise"
        };

        Exercise? capturedExercise = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedExercise);
        Assert.Equal("Deadlift", capturedExercise.Name);
        Assert.Equal("Back", capturedExercise.MuscleGroup);
        Assert.Equal("Advanced", capturedExercise.Difficulty);
        Assert.Equal("Barbell", capturedExercise.Equipment);
        Assert.Equal("Compound back and leg exercise", capturedExercise.Description);
    }

    [Theory]
    [InlineData("Push-ups", "Chest", "Beginner", "Bodyweight")]
    [InlineData("Squats", "Legs", "Intermediate", "Barbell")]
    [InlineData("Pull-ups", "Back", "Advanced", "Pull-up bar")]
    public async Task Handle_VariousExercises_CreatesCorrectly(
        string name, 
        string muscleGroup, 
        string difficulty, 
        string equipment)
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = name,
            MuscleGroup = muscleGroup,
            Difficulty = difficulty,
            Equipment = equipment,
            Description = $"Exercise for {muscleGroup}"
        };

        Exercise? capturedExercise = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedExercise);
        Assert.Equal(name, capturedExercise.Name);
        Assert.Equal(muscleGroup, capturedExercise.MuscleGroup);
        Assert.Equal(difficulty, capturedExercise.Difficulty);
        Assert.Equal(equipment, capturedExercise.Equipment);
    }

    [Fact]
    public async Task Handle_ExerciseWithoutDescription_CreatesSuccessfully()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Plank",
            MuscleGroup = "Core",
            Difficulty = "Beginner",
            Equipment = "None",
            Description = null
        };

        Exercise? capturedExercise = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Exercise>()))
            .Callback<Exercise>(e => capturedExercise = e);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedExercise);
        Assert.Null(capturedExercise.Description);
    }
}

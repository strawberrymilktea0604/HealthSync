using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class DeleteExerciseCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly DeleteExerciseCommandHandler _handler;

    public DeleteExerciseCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteExerciseCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteExercise_WhenExerciseExistsAndNotUsed()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 1,
            Name = "Bench Press",
            MuscleGroup = "Chest"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var exerciseSessions = new List<ExerciseSession>();
        var mockSessions = exerciseSessions.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ExerciseSessions).Returns(mockSessions);

        var command = new DeleteExerciseCommand { ExerciseId = 1 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _contextMock.Verify(c => c.Remove(exercise), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exercises = new List<Exercise>();
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var command = new DeleteExerciseCommand { ExerciseId = 999 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _contextMock.Verify(c => c.Remove(It.IsAny<Exercise>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExerciseIsBeingUsed()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 1,
            Name = "Squat",
            MuscleGroup = "Legs"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var exerciseSessions = new List<ExerciseSession>
        {
            new ExerciseSession { ExerciseId = 1, WorkoutLogId = 1 }
        };
        var mockSessions = exerciseSessions.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ExerciseSessions).Returns(mockSessions);

        var command = new DeleteExerciseCommand { ExerciseId = 1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Không thể xóa bài tập đang được sử dụng", exception.Message);
        _contextMock.Verify(c => c.Remove(It.IsAny<Exercise>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCheckExerciseUsage_BeforeDeleting()
    {
        // Arrange
        var exercise = new Exercise
        {
            ExerciseId = 2,
            Name = "Deadlift",
            MuscleGroup = "Back"
        };

        var exercises = new List<Exercise> { exercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var exerciseSessions = new List<ExerciseSession>();
        var mockSessions = exerciseSessions.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ExerciseSessions).Returns(mockSessions);

        var command = new DeleteExerciseCommand { ExerciseId = 2 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify that usage check was performed
        _contextMock.Verify(c => c.ExerciseSessions, Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteOtherExercises_WhenDeletingOne()
    {
        // Arrange
        var exerciseToDelete = new Exercise
        {
            ExerciseId = 1,
            Name = "Push-ups",
            MuscleGroup = "Chest"
        };

        var otherExercise = new Exercise
        {
            ExerciseId = 2,
            Name = "Pull-ups",
            MuscleGroup = "Back"
        };

        var exercises = new List<Exercise> { exerciseToDelete, otherExercise };
        var mockExercises = exercises.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.Exercises).Returns(mockExercises);

        var exerciseSessions = new List<ExerciseSession>();
        var mockSessions = exerciseSessions.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ExerciseSessions).Returns(mockSessions);

        var command = new DeleteExerciseCommand { ExerciseId = 1 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Only the target exercise should be removed
        _contextMock.Verify(c => c.Remove(exerciseToDelete), Times.Once);
        _contextMock.Verify(c => c.Remove(otherExercise), Times.Never);
    }
}

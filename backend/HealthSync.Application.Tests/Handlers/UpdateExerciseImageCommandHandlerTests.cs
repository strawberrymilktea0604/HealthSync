using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateExerciseImageCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UpdateExerciseImageCommandHandler _handler;

    public UpdateExerciseImageCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateExerciseImageCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateInternalImage_WhenExerciseExists()
    {
        // Arrange
        var command = new UpdateExerciseImageCommand
        {
            ExerciseId = 1,
            ImageUrl = "http://example.com/new-image.jpg"
        };

        var exercise = new Exercise
        {
            ExerciseId = 1,
            ImageUrl = "http://example.com/old-image.jpg"
        };

        var exercisesList = new List<Exercise> { exercise };
        var exercisesMock = exercisesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Exercises).Returns(exercisesMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.Equal("http://example.com/new-image.jpg", exercise.ImageUrl);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenExerciseNotFound()
    {
        // Arrange
        var command = new UpdateExerciseImageCommand
        {
            ExerciseId = 999,
            ImageUrl = "http://example.com/any.jpg"
        };

        var exercisesList = new List<Exercise>();
        var exercisesMock = exercisesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Exercises).Returns(exercisesMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Không tìm thấy bài tập", exception.Message);
        
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

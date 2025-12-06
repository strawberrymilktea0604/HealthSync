using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class CreateGoalCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateGoalCommandHandler _handler;

    public CreateGoalCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new CreateGoalCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidGoal_ReturnsGoalResponse()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_loss",
            TargetValue = 65.0m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(3),
            Notes = "Lose 5kg in 3 months"
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => { if (capturedGoal != null) capturedGoal.GoalId = 456; })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.GoalId);
        Assert.Equal("weight_loss", result.Type);
        Assert.Equal(65.0m, result.TargetValue);
        Assert.Equal("active", result.Status);
        Assert.NotNull(result.ProgressRecords);
        Assert.Empty(result.ProgressRecords);
        
        _mockContext.Verify(x => x.Add(It.IsAny<Goal>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GoalWithoutNotes_CreatesGoalSuccessfully()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 2,
            Type = "muscle_gain",
            TargetValue = 75.0m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(6),
            Notes = null
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("muscle_gain", result.Type);
        Assert.Null(capturedGoal?.Notes);
    }

    [Fact]
    public async Task Handle_ValidGoal_SetsActiveStatusByDefault()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_maintenance",
            TargetValue = 70.0m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(1)
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedGoal);
        Assert.Equal("active", capturedGoal.Status);
    }

    [Theory]
    [InlineData("weight_loss", 60.0)]
    [InlineData("muscle_gain", 80.0)]
    [InlineData("fat_loss", 15.0)]
    public async Task Handle_DifferentGoalTypes_CreatesGoalCorrectly(string goalType, decimal targetValue)
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = goalType,
            TargetValue = targetValue,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(2)
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedGoal);
        Assert.Equal(goalType, capturedGoal.Type);
        Assert.Equal(targetValue, capturedGoal.TargetValue);
    }
}

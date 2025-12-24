using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;

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
    public async Task Handle_ValidGoal_CreatesSuccessfully()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_loss",
            TargetValue = 65.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(3),
            Notes = "Lose 5kg for summer"
        };

        Goal? capturedGoal = null;
        _mockContext.Setup(x => x.Add(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (capturedGoal != null)
                {
                    capturedGoal.GoalId = 42;
                }
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.GoalId);
        Assert.Equal("weight_loss", result.Type);
        Assert.Equal(65.0m, result.TargetValue);
        Assert.Equal("active", result.Status);
        Assert.Equal("Lose 5kg for summer", result.Notes);
        Assert.Empty(result.ProgressRecords);
    }

    [Fact]
    public async Task Handle_NewGoal_SetsStatusToActive()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "muscle_gain",
            TargetValue = 75.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(6)
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

    [Fact]
    public async Task Handle_ValidGoal_CallsContextAddOnce()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "body_fat_reduction",
            TargetValue = 15.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(4)
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.Add(It.IsAny<Goal>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidGoal_CallsSaveChangesAsync()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "endurance_improvement",
            TargetValue = 10.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(2)
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GoalWithoutNotes_CreatesSuccessfully()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_maintenance",
            TargetValue = 70.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(1),
            Notes = null
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
        Assert.Null(capturedGoal.Notes);
    }

    [Fact]
    public async Task Handle_GoalDatesPreserved_SavesCorrectDates()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 6, 30);
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "strength_gain",
            TargetValue = 100.0m,
            StartDate = startDate,
            EndDate = endDate
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
        Assert.Equal(startDate, capturedGoal.StartDate);
        Assert.Equal(endDate, capturedGoal.EndDate);
    }

    [Fact]
    public async Task Handle_CorrectUserId_AssignsToGoal()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 99,
            Type = "flexibility_improvement",
            TargetValue = 30.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(3)
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
        Assert.Equal(99, capturedGoal.UserId);
    }

    [Fact]
    public async Task Handle_NewGoal_ReturnsEmptyProgressRecords()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "cardio_endurance",
            TargetValue = 5.0m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(2)
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ProgressRecords);
        Assert.Empty(result.ProgressRecords);
    }

    [Fact]
    public async Task Handle_DecimalTargetValue_PreservesPrecision()
    {
        // Arrange
        var command = new CreateGoalCommand
        {
            UserId = 1,
            Type = "weight_loss",
            TargetValue = 68.75m, // Decimal with precision
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(3)
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
        Assert.Equal(68.75m, capturedGoal.TargetValue);
    }
}

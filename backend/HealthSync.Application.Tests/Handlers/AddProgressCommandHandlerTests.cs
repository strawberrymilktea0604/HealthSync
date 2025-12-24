using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;

namespace HealthSync.Application.Tests.Handlers;

public class AddProgressCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly AddProgressCommandHandler _handler;

    public AddProgressCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new AddProgressCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidProgress_AddsProgressRecordSuccessfully()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "weight_loss",
                TargetValue = 65.0m,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(2),
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 68.5m,
            WeightKg = 68.5m,
            Notes = "Lost 1kg this week"
        };

        ProgressRecord? capturedRecord = null;
        _mockContext.Setup(x => x.Add(It.IsAny<ProgressRecord>()))
            .Callback<ProgressRecord>(pr => capturedRecord = pr);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (capturedRecord != null)
                {
                    capturedRecord.ProgressRecordId = 50;
                }
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ProgressRecord);
        Assert.Equal(50, result.ProgressRecord.ProgressRecordId);
        Assert.Equal(68.5m, result.ProgressRecord.Value);
        Assert.Equal(68.5m, result.ProgressRecord.WeightKg);
        Assert.Equal("Lost 1kg this week", result.ProgressRecord.Notes);
    }

    [Fact]
    public async Task Handle_GoalNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var goals = new List<Goal>(); // Empty list

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 999, // Non-existent
            RecordDate = DateTime.Now,
            Value = 70.0m
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GoalBelongsToDifferentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 2, // Different user
                Type = "weight_loss",
                TargetValue = 65.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1, // User 1 trying to add progress to User 2's goal
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 70.0m
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallsContextAddOnce()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "muscle_gain",
                TargetValue = 75.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 72.5m,
            WeightKg = 72.5m
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.Add(It.IsAny<ProgressRecord>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsSaveChangesAsync()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "body_fat_reduction",
                TargetValue = 15.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 18.0m
        };

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOptionalFields_SavesAllData()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "weight_loss",
                TargetValue = 65.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 70.0m,
            WeightKg = 70.0m,
            WaistCm = 85.5m,
            Notes = "Feeling good"
        };

        ProgressRecord? capturedRecord = null;
        _mockContext.Setup(x => x.Add(It.IsAny<ProgressRecord>()))
            .Callback<ProgressRecord>(pr => capturedRecord = pr);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecord);
        Assert.Equal(1, capturedRecord.GoalId);
        Assert.Equal(70.0m, capturedRecord.Value);
        Assert.Equal(70.0m, capturedRecord.WeightKg);
        Assert.Equal(85.5m, capturedRecord.WaistCm);
        Assert.Equal("Feeling good", capturedRecord.Notes);
    }

    [Fact]
    public async Task Handle_WithoutOptionalWeightAndWaist_SetsToZero()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "endurance",
                TargetValue = 10.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 8.0m,
            WeightKg = null,
            WaistCm = null
        };

        ProgressRecord? capturedRecord = null;
        _mockContext.Setup(x => x.Add(It.IsAny<ProgressRecord>()))
            .Callback<ProgressRecord>(pr => capturedRecord = pr);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecord);
        Assert.Equal(0, capturedRecord.WeightKg);
        Assert.Equal(0, capturedRecord.WaistCm);
    }

    [Fact]
    public async Task Handle_PreservesRecordDate_Correctly()
    {
        // Arrange
        var specificDate = new DateTime(2024, 12, 23, 8, 30, 0);
        var goals = new List<Goal>
        {
            new Goal
            {
                GoalId = 1,
                UserId = 1,
                Type = "weight_loss",
                TargetValue = 65.0m,
                Status = "active"
            }
        };

        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = specificDate,
            Value = 67.5m
        };

        ProgressRecord? capturedRecord = null;
        _mockContext.Setup(x => x.Add(It.IsAny<ProgressRecord>()))
            .Callback<ProgressRecord>(pr => capturedRecord = pr);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecord);
        Assert.Equal(specificDate, capturedRecord.RecordDate);
    }

    [Fact]
    public async Task Handle_ExceptionMessageCorrect_WhenGoalNotFound()
    {
        // Arrange
        var goals = new List<Goal>();
        var mockGoals = goals.AsQueryable().BuildMock();
        _mockContext.Setup(x => x.Goals).Returns(mockGoals);

        var command = new AddProgressCommand
        {
            UserId = 1,
            GoalId = 1,
            RecordDate = DateTime.Now,
            Value = 70.0m
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Goal not found or does not belong to the user.", exception.Message);
    }
}

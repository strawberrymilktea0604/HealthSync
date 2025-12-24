using HealthSync.Application.Handlers;
using HealthSync.Application.Queries;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using System.Linq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GetGoalsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetGoalsQueryHandler _handler;

    public GetGoalsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetGoalsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGoalsForUser()
    {
        // Arrange
        var userId = 1;
        var goals = new List<Goal>
        {
            new Goal { GoalId = 1, UserId = userId, Type = "weight_loss", TargetValue = 10, StartDate = DateTime.UtcNow },
            new Goal { GoalId = 2, UserId = userId, Type = "muscle_gain", TargetValue = 5, StartDate = DateTime.UtcNow }
        };

        var mockGoals = goals.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.Goals).Returns(mockGoals.Object);

        var query = new GetGoalsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Goals.Count);
        Assert.Equal("weight_loss", result.Goals[0].Type);
        Assert.Equal("muscle_gain", result.Goals[1].Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoGoals()
    {
        // Arrange
        var userId = 1;
        var goals = new List<Goal>();

        var mockGoals = goals.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.Goals).Returns(mockGoals.Object);

        var query = new GetGoalsQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Goals);
    }
}
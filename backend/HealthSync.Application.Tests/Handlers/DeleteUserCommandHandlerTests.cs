using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new DeleteUserCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_DeletesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var user = new ApplicationUser
        {
            UserId = userId,
            Email = "delete@example.com",
            IsActive = true
        };

        var mockUserSet = CreateMockDbSet(new List<ApplicationUser> { user });
        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);
        _mockContext.Setup(x => x.Remove(It.IsAny<ApplicationUser>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserCommand { UserId = userId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockContext.Verify(x => x.Remove(It.Is<ApplicationUser>(u => u.UserId == userId)), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsException()
    {
        // Arrange
        var mockUserSet = CreateMockDbSet(new List<ApplicationUser>());
        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);

        var command = new DeleteUserCommand { UserId = 999 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Contains("User with ID 999 not found", exception.Message);
        _mockContext.Verify(x => x.Remove(It.IsAny<ApplicationUser>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    public async Task Handle_VariousUserIds_DeletesCorrectUser(int userId)
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "user1@example.com" },
            new ApplicationUser { UserId = 42, Email = "user42@example.com" },
            new ApplicationUser { UserId = 100, Email = "user100@example.com" }
        };

        var mockUserSet = CreateMockDbSet(users);
        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);
        _mockContext.Setup(x => x.Remove(It.IsAny<ApplicationUser>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteUserCommand { UserId = userId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockContext.Verify(x => x.Remove(It.Is<ApplicationUser>(u => u.UserId == userId)), Times.Once);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return mockSet;
    }
}

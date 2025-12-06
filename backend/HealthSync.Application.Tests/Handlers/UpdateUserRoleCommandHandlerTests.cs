using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly UpdateUserRoleCommandHandler _handler;

    public UpdateUserRoleCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new UpdateUserRoleCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_UpdatesRoleSuccessfully()
    {
        // Arrange
        var userId = 1;
        var newRoleName = "Admin";

        var user = new ApplicationUser
        {
            UserId = userId,
            Email = "test@example.com",
            IsActive = true,
            Profile = new UserProfile { FullName = "Test User" },
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = 2, Role = new Role { Id = 2, RoleName = "Customer" } }
            }
        };

        var newRole = new Role { Id = 1, RoleName = newRoleName };

        var mockUserSet = CreateMockDbSet(new List<ApplicationUser> { user });
        var mockRoleSet = CreateMockDbSet(new List<Role> { newRole });

        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);
        _mockContext.Setup(x => x.Roles).Returns(mockRoleSet.Object);
        _mockContext.Setup(x => x.Remove(It.IsAny<UserRole>()));
        _mockContext.Setup(x => x.Add(It.IsAny<UserRole>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            Role = newRoleName
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(newRoleName, result.Role);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("Test User", result.FullName);
        
        _mockContext.Verify(x => x.Remove(It.IsAny<UserRole>()), Times.Once);
        _mockContext.Verify(x => x.Add(It.IsAny<UserRole>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var mockUserSet = CreateMockDbSet(new List<ApplicationUser>());
        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);

        var command = new UpdateUserRoleCommand
        {
            UserId = 999,
            Role = "Admin"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Contains("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task Handle_RoleNotFound_ThrowsException()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "test@example.com",
            UserRoles = new List<UserRole>()
        };

        var mockUserSet = CreateMockDbSet(new List<ApplicationUser> { user });
        var mockRoleSet = CreateMockDbSet(new List<Role>());

        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);
        _mockContext.Setup(x => x.Roles).Returns(mockRoleSet.Object);

        var command = new UpdateUserRoleCommand
        {
            UserId = 1,
            Role = "InvalidRole"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Contains("Role 'InvalidRole' not found", exception.Message);
    }

    [Fact]
    public async Task Handle_UserWithoutExistingRole_AddsNewRole()
    {
        // Arrange
        var userId = 1;
        var user = new ApplicationUser
        {
            UserId = userId,
            Email = "newuser@example.com",
            IsActive = true,
            Profile = new UserProfile { FullName = "New User" },
            UserRoles = new List<UserRole>() // No existing roles
        };

        var newRole = new Role { Id = 2, RoleName = "Customer" };

        var mockUserSet = CreateMockDbSet(new List<ApplicationUser> { user });
        var mockRoleSet = CreateMockDbSet(new List<Role> { newRole });

        _mockContext.Setup(x => x.ApplicationUsers).Returns(mockUserSet.Object);
        _mockContext.Setup(x => x.Roles).Returns(mockRoleSet.Object);
        _mockContext.Setup(x => x.Add(It.IsAny<UserRole>()));
        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateUserRoleCommand
        {
            UserId = userId,
            Role = "Customer"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Customer", result.Role);
        _mockContext.Verify(x => x.Remove(It.IsAny<UserRole>()), Times.Never);
        _mockContext.Verify(x => x.Add(It.IsAny<UserRole>()), Times.Once);
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

// Helper classes for async query testing
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;
}

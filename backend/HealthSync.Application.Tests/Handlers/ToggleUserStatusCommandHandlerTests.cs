using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class ToggleUserStatusCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly ToggleUserStatusCommandHandler _handler;

    public ToggleUserStatusCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ToggleUserStatusCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_WhenUserExists_AndNotSelf()
    {
        // Arrange
        var userId = 10;
        var currentUserId = 99; // Different ID
        var isActive = false;

        var user = new ApplicationUser 
        { 
            UserId = userId, 
            IsActive = true 
        };

        var users = new List<ApplicationUser> { user };
        _contextMock.Setup(c => c.ApplicationUsers).Returns(users.AsQueryable().BuildMock());

        var command = new ToggleUserStatusCommand 
        { 
            UserId = userId, 
            IsActive = isActive, 
            CurrentUserId = currentUserId 
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.False(user.IsActive);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserTriesToSelfToggle()
    {
        // Arrange
        var userId = 10;
        var currentUserId = 10; // Same ID
        
        var command = new ToggleUserStatusCommand 
        { 
            UserId = userId, 
            IsActive = false, 
            CurrentUserId = currentUserId 
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var userId = 10;
        var currentUserId = 99;

        _contextMock.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser>().AsQueryable().BuildMock());

        var command = new ToggleUserStatusCommand 
        { 
            UserId = userId, 
            IsActive = false, 
            CurrentUserId = currentUserId 
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
            
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

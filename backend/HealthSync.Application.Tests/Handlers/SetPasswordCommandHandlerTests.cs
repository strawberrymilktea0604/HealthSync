using HealthSync.Application.Commands;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class SetPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly SetPasswordCommandHandler _handler;

    public SetPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _authServiceMock = new Mock<IAuthService>();
        _handler = new SetPasswordCommandHandler(_contextMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSetPassword_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = null!
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _authServiceMock.Setup(a => a.HashPassword("NewPassword123!"))
            .Returns("hashed-password");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new SetPasswordCommand { UserId = 1, Password = "NewPassword123!" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("hashed-password", user.PasswordHash);
        _authServiceMock.Verify(a => a.HashPassword("NewPassword123!"), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var users = new List<ApplicationUser>();
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var command = new SetPasswordCommand { UserId = 999, Password = "NewPassword123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User not found", exception.Message);
        _authServiceMock.Verify(a => a.HashPassword(It.IsAny<string>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReplaceExistingPassword()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = "old-hashed-password"
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _authServiceMock.Setup(a => a.HashPassword("NewPassword456!"))
            .Returns("new-hashed-password");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new SetPasswordCommand { UserId = 1, Password = "NewPassword456!" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual("old-hashed-password", user.PasswordHash);
        Assert.Equal("new-hashed-password", user.PasswordHash);
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordBeforeStoring()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = null!
        };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        var plainPassword = "PlainPassword123!";
        var hashedPassword = "securely-hashed-password";
        _authServiceMock.Setup(a => a.HashPassword(plainPassword)).Returns(hashedPassword);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new SetPasswordCommand { UserId = 1, Password = plainPassword };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(plainPassword, user.PasswordHash); // Should not store plain text
        Assert.Equal(hashedPassword, user.PasswordHash);
        _authServiceMock.Verify(a => a.HashPassword(plainPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges_AfterSettingPassword()
    {
        // Arrange
        var user = new ApplicationUser { UserId = 1, Email = "user@test.com" };
        var users = new List<ApplicationUser> { user };
        var mockUsers = users.AsQueryable().BuildMock();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers);

        _authServiceMock.Setup(a => a.HashPassword(It.IsAny<string>())).Returns("hashed");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new SetPasswordCommand { UserId = 1, Password = "Password123!" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

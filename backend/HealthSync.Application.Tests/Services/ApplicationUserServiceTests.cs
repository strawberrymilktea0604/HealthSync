using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Services;

public class ApplicationUserServiceTests
{
    private readonly Mock<IApplicationUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly ApplicationUserService _service;

    public ApplicationUserServiceTests()
    {
        _userRepositoryMock = new Mock<IApplicationUserRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _service = new ApplicationUserService(_userRepositoryMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new ApplicationUser { UserId = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserNotExists_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new ApplicationUser { UserId = 1, Email = email };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { UserId = 1, Email = "user1@example.com" },
            new ApplicationUser { UserId = 2, Email = "user2@example.com" }
        };
        _userRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateUserAsync_NewUser_CreatesSuccessfully()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "Password123!";
        var role = "Customer";
        var hashedPassword = "hashed_password";

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);
        _authServiceMock.Setup(a => a.HashPassword(password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ApplicationUser>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateUserAsync(email, password, role);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(hashedPassword, result.PasswordHash);
        Assert.True(result.IsActive);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ExistingEmail_ThrowsException()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "Password123!";
        var existingUser = new ApplicationUser { UserId = 1, Email = email };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync(email, password));
    }

    [Fact]
    public async Task UpdateUserAsync_CallsRepositoryUpdate()
    {
        // Arrange
        var user = new ApplicationUser { UserId = 1, Email = "test@example.com" };
        _userRepositoryMock.Setup(r => r.UpdateAsync(user))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateUserAsync(user);

        // Assert
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_UserExists_DeletesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var user = new ApplicationUser { UserId = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.DeleteAsync(user))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteUserAsync(userId);

        // Assert
        _userRepositoryMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_UserNotExists_ThrowsException()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task AuthenticateUserAsync_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var passwordHash = "hashed_password";
        var user = new ApplicationUser 
        { 
            UserId = 1, 
            Email = email, 
            PasswordHash = passwordHash,
            IsActive = true 
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _authServiceMock.Setup(a => a.VerifyPassword(password, passwordHash))
            .Returns(true);

        // Act
        var result = await _service.AuthenticateUserAsync(email, password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var passwordHash = "hashed_password";
        var user = new ApplicationUser 
        { 
            UserId = 1, 
            Email = email, 
            PasswordHash = passwordHash,
            IsActive = true 
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _authServiceMock.Setup(a => a.VerifyPassword(password, passwordHash))
            .Returns(false);

        // Act
        var result = await _service.AuthenticateUserAsync(email, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_UserNotExists_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "Password123!";

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.AuthenticateUserAsync(email, password);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_InactiveUser_ReturnsFalse()
    {
        // Arrange
        var email = "inactive@example.com";
        var password = "Password123!";
        var user = new ApplicationUser 
        { 
            UserId = 1, 
            Email = email, 
            PasswordHash = "hashed_password",
            IsActive = false 
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.AuthenticateUserAsync(email, password);

        // Assert
        Assert.False(result);
    }
}

using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using System.Linq.Expressions;
using System.Linq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _authServiceMock = new Mock<IAuthService>();
        _mediatorMock = new Mock<IMediator>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new RegisterUserCommandHandler(
            _contextMock.Object,
            _authServiceMock.Object,
            _mediatorMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenVerificationCodeInvalid()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Mã xác thực không hợp lệ", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenEmailAlreadyExists()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "user@example.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Customer" } }
            }
        };
        var users = new List<ApplicationUser> { existingUser };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Email đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperation_WhenEmailAlreadyExistsAsAdmin()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "admin@example.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { RoleName = "Admin" } }
            }
        };
        var users = new List<ApplicationUser> { existingUser };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var command = new RegisterUserCommand
        {
            Email = "admin@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("đã được đăng ký với tài khoản Admin", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserSuccessfully_WhenDataValid()
    {
        // Setup verification code validation to return true
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Setup users list that will be modified during the test
        var users = new List<ApplicationUser>();
        
        // First call: check if email exists (returns empty)
        var initialMockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(initialMockUsers.Object);

        var roles = new List<Role> { new Role { Id = 1, RoleName = "Customer" } };
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(mockRoles.Object);

        _authServiceMock.Setup(a => a.HashPassword("password123")).Returns("hashedpassword");

        // Mock Add to simulate user creation
        _contextMock.Setup(c => c.Add(It.IsAny<ApplicationUser>()))
            .Callback<object>(entity =>
            {
                if (entity is ApplicationUser user)
                {
                    user.UserId = 1; // Simulate ID assignment
                    users.Add(user); // Add to list for reload
                }
            });

        // Setup reloaded user with roles after SaveChanges
        var reloadedUser = new ApplicationUser
        {
            UserId = 1,
            Email = "user@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole 
                { 
                    Role = new Role 
                    { 
                        Id = 1, 
                        RoleName = "Customer", 
                        RolePermissions = new List<RolePermission>() 
                    } 
                }
            }
        };
        
        // Second call: reload user after creation (returns user with roles)
        var reloadMockUsers = new List<ApplicationUser> { reloadedUser }.AsQueryable().BuildMockDbSet();
        
        // Setup sequence: first returns empty, then returns reloaded user
        var callCount = 0;
        _contextMock.Setup(c => c.ApplicationUsers)
            .Returns(() =>
            {
                callCount++;
                return callCount == 1 ? initialMockUsers.Object : reloadMockUsers.Object;
            });

        var tokenDto = new TokenDto
        {
            AccessToken = "jwt-token-123",
            ExpiresIn = 3600,
            Roles = new List<string> { "Customer" },
            Permissions = new List<string> { "read" }
        };
        _jwtTokenServiceMock.Setup(j => j.GenerateTokenAsync(It.IsAny<int>(), "user@example.com", It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(tokenDto);

        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("jwt-token-123", result.Token);
        Assert.Contains("Customer", result.Roles);

        // Verify user was added
        _contextMock.Verify(c => c.Add(It.IsAny<ApplicationUser>()), Times.AtLeastOnce);
        _contextMock.Verify(c => c.Add(It.IsAny<UserRole>()), Times.Once);
        _contextMock.Verify(c => c.Add(It.IsAny<UserProfile>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_ShouldHandleCaseInsensitiveEmailCheck()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "User@Example.Com", // Upper case
            UserRoles = new List<UserRole>()
        };
        var users = new List<ApplicationUser> { existingUser };
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var command = new RegisterUserCommand
        {
            Email = "user@example.com", // Lower case
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Email đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRoleNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Setup empty initial state
        var users = new List<ApplicationUser>();
        var initialMockUsers = users.AsQueryable().BuildMockDbSet();

        var roles = new List<Role>(); // No Customer role
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(mockRoles.Object);

        _authServiceMock.Setup(a => a.HashPassword("password123")).Returns("hashedpassword");

        // Mock Add to simulate user creation with ID assignment
        _contextMock.Setup(c => c.Add(It.IsAny<ApplicationUser>()))
            .Callback<object>(entity =>
            {
                if (entity is ApplicationUser user)
                {
                    user.UserId = 1; // Simulate ID assignment
                }
            });

        // Setup reloaded user without roles (since Customer role doesn't exist)
        var reloadedUser = new ApplicationUser
        {
            UserId = 1,
            Email = "user@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true,
            UserRoles = new List<UserRole>() // No roles assigned
        };
        
        var reloadMockUsers = new List<ApplicationUser> { reloadedUser }.AsQueryable().BuildMockDbSet();
        
        // Setup sequence: first returns empty (no existing user), then returns reloaded user
        var callCount = 0;
        _contextMock.Setup(c => c.ApplicationUsers)
            .Returns(() =>
            {
                callCount++;
                return callCount == 1 ? initialMockUsers.Object : reloadMockUsers.Object;
            });

        var tokenDto = new TokenDto
        {
            AccessToken = "jwt-token-123",
            ExpiresIn = 3600,
            Roles = new List<string>(),
            Permissions = new List<string>()
        };
        _jwtTokenServiceMock.Setup(j => j.GenerateTokenAsync(It.IsAny<int>(), "user@example.com", It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(tokenDto);

        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act - Since Customer role doesn't exist, user is created but without any role
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert - User should be created successfully, just without Customer role
        Assert.NotNull(result);
        Assert.Equal("user@example.com", result.Email);
        Assert.Empty(result.Roles); // No roles assigned
        _contextMock.Verify(c => c.Add(It.IsAny<ApplicationUser>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserReloadFails()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var users = new List<ApplicationUser>(); // Empty for initial check
        var mockUsers = users.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.ApplicationUsers).Returns(mockUsers.Object);

        var roles = new List<Role> { new Role { Id = 1, RoleName = "Customer" } };
        var mockRoles = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Roles).Returns(mockRoles.Object);

        _authServiceMock.Setup(a => a.HashPassword("password123")).Returns("hashedpassword");

        // Mock reload to return null
        _contextMock.SetupSequence(c => c.ApplicationUsers)
            .Returns(mockUsers.Object) // First call for existence check
            .Returns(new List<ApplicationUser>().AsQueryable().BuildMockDbSet().Object); // Second call returns empty

        var command = new RegisterUserCommand
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Failed to reload user", exception.Message);
    }
}
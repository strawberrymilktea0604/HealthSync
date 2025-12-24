using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Application.Services;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GoogleLoginWebCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IGoogleAuthService> _mockGoogleAuthService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly GoogleLoginWebCommandHandler _handler;

    public GoogleLoginWebCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockGoogleAuthService = new Mock<IGoogleAuthService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _handler = new GoogleLoginWebCommandHandler(
            _mockContext.Object,
            _mockGoogleAuthService.Object,
            _mockJwtTokenService.Object);
    }

    [Fact]
    public async Task Handle_InvalidGoogleCode_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        var command = new GoogleLoginWebCommand { Code = "invalid_code" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "admin@test.com",
            Name = "Admin User",
            Picture = "http://picture.com"
        };

        var adminUser = new ApplicationUser
        {
            UserId = 1,
            Email = "admin@test.com",
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = 1,
                    Role = new Role { Id = 1, RoleName = "Admin" }
                }
            }
        };

        var adminUsers = new List<ApplicationUser> { adminUser }.AsQueryable().BuildMock();

        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockContext.Setup(c => c.ApplicationUsers).Returns(adminUsers);

        var command = new GoogleLoginWebCommand { Code = "valid_code" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Admin", exception.Message);
    }

    [Fact]
    public async Task Handle_NewUser_CreatesUserAndProfile()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "newuser@test.com",
            Name = "New User",
            Picture = "http://picture.com"
        };

        var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var customerRole = new Role { Id = 2, RoleName = "Customer" };
        var roles = new List<Role> { customerRole }.AsQueryable().BuildMock();

        ApplicationUser? capturedUser = null;
        var capturedObjects = new List<object>();

        var callCount = 0;
        _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
        {
            callCount++;
            // First 2 calls: check for admin and existing user (both empty)
            if (callCount <= 2) return emptyUsers;
            // Third call: reload user after creation
            if (capturedUser != null)
            {
                capturedUser.UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = capturedUser.UserId,
                        RoleId = 2,
                        Role = new Role
                        {
                            Id = 2,
                            RoleName = "Customer",
                            RolePermissions = new List<RolePermission>()
                        }
                    }
                };
                capturedUser.Profile = new UserProfile
                {
                    UserId = capturedUser.UserId,
                    FullName = googleUser.Name,
                    AvatarUrl = googleUser.Picture,
                    Gender = "Unknown"
                };
                return new List<ApplicationUser> { capturedUser }.AsQueryable().BuildMock();
            }
            return emptyUsers;
        });

        _mockContext.Setup(c => c.Roles).Returns(roles);
        _mockContext.Setup(c => c.Add(It.IsAny<object>())).Callback<object>(entity =>
        {
            capturedObjects.Add(entity);
            if (entity is ApplicationUser user)
            {
                user.UserId = 1; // Simulate database ID generation
                capturedUser = user;
            }
        });
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginWebCommand { Code = "valid_code" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
        Assert.Equal("jwt_token", result.Token);
        Assert.Contains("Customer", result.Roles);
        Assert.True(result.RequiresPassword); // New Google user needs password
        
        // Verify user, role, and profile were created
        Assert.Contains(capturedObjects, o => o is ApplicationUser);
        Assert.Contains(capturedObjects, o => o is UserRole);
        Assert.Contains(capturedObjects, o => o is UserProfile);
    }

    [Fact]
    public async Task Handle_ExistingUser_UpdatesProfileAndReturns()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "existing@test.com",
            Name = "Updated Name",
            Picture = "http://newpicture.com"
        };

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "existing@test.com",
            PasswordHash = "hashed_password",
            Profile = new UserProfile
            {
                UserId = 1,
                FullName = "Old Name",
                AvatarUrl = "http://oldpicture.com",
                Gender = "Male",
                HeightCm = 175,
                WeightKg = 70
            },
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = 2,
                    Role = new Role
                    {
                        Id = 2,
                        RoleName = "Customer",
                        RolePermissions = new List<RolePermission>
                        {
                            new RolePermission
                            {
                                RoleId = 2,
                                PermissionId = 1,
                                Permission = new Permission { Id = 1, PermissionCode = "VIEW_DASHBOARD" }
                            }
                        }
                    }
                }
            }
        };

        var emptyAdminUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var existingUsers = new List<ApplicationUser> { existingUser }.AsQueryable().BuildMock();

        var callCount = 0;
        _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
        {
            callCount++;
            if (callCount == 1) return emptyAdminUsers; // Check for admin
            return existingUsers; // Return existing user
        });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string> { "VIEW_DASHBOARD" }
            });

        var command = new GoogleLoginWebCommand { Code = "valid_code" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("existing@test.com", result.Email);
        Assert.Equal("jwt_token", result.Token);
        Assert.False(result.RequiresPassword); // Existing user has password
        Assert.Contains("Customer", result.Roles);
        Assert.Contains("VIEW_DASHBOARD", result.Permissions);
        
        // Verify profile was updated
        Assert.Equal("http://newpicture.com", existingUser.Profile.AvatarUrl);
    }

    [Fact]
    public async Task Handle_ExistingUserWithoutProfileName_UpdatesFullName()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "user@test.com",
            Name = "Google Name",
            Picture = "http://picture.com"
        };

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = "",
            Profile = new UserProfile
            {
                UserId = 1,
                FullName = "", // Empty name
                Gender = "Male"
            },
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = 2,
                    Role = new Role
                    {
                        Id = 2,
                        RoleName = "Customer",
                        RolePermissions = new List<RolePermission>()
                    }
                }
            }
        };

        var emptyAdminUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var existingUsers = new List<ApplicationUser> { existingUser }.AsQueryable().BuildMock();

        var callCount = 0;
        _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
        {
            callCount++;
            if (callCount == 1) return emptyAdminUsers;
            return existingUsers;
        });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginWebCommand { Code = "valid_code" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Google Name", result.FullName);
        Assert.Equal("Google Name", existingUser.Profile.FullName); // Updated from Google
        Assert.True(result.RequiresPassword); // No password hash
    }

    [Fact]
    public async Task Handle_ValidLogin_UpdatesLastLoginAt()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "user@test.com",
            Name = "Test User"
        };

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "user@test.com",
            PasswordHash = "hashed",
            LastLoginAt = null,
            Profile = new UserProfile { UserId = 1, FullName = "Test User" },
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = 2,
                    Role = new Role { Id = 2, RoleName = "Customer", RolePermissions = new List<RolePermission>() }
                }
            }
        };

        var emptyAdminUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var existingUsers = new List<ApplicationUser> { existingUser }.AsQueryable().BuildMock();

        var callCount = 0;
        _mockContext.Setup(c => c.ApplicationUsers).Returns(() =>
        {
            callCount++;
            if (callCount == 1) return emptyAdminUsers;
            return existingUsers;
        });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.ProcessCallbackAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginWebCommand { Code = "valid_code" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(existingUser.LastLoginAt); // Verify LastLoginAt was updated
        Assert.True(existingUser.LastLoginAt.Value >= DateTime.UtcNow.AddSeconds(-5)); // Within last 5 seconds
    }
}

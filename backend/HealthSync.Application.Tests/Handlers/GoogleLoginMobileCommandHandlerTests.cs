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

public class GoogleLoginMobileCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IGoogleAuthService> _mockGoogleAuthService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly GoogleLoginMobileCommandHandler _handler;

    public GoogleLoginMobileCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockGoogleAuthService = new Mock<IGoogleAuthService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _handler = new GoogleLoginMobileCommandHandler(
            _mockContext.Object,
            _mockGoogleAuthService.Object,
            _mockJwtTokenService.Object);
    }

    [Fact]
    public async Task Handle_InvalidIdToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        var command = new GoogleLoginMobileCommand { IdToken = "invalid_token" };

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
            Name = "Admin User"
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

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockContext.Setup(c => c.ApplicationUsers).Returns(adminUsers);

        var command = new GoogleLoginMobileCommand { IdToken = "valid_token" };

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
            Name = "New Mobile User"
        };

        var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var customerRole = new Role { Id = 2, RoleName = "Customer" };
        var roles = new List<Role> { customerRole }.AsQueryable().BuildMock();

        ApplicationUser? capturedUser = null;
        var capturedObjects = new List<object>();

        _mockContext.Setup(c => c.ApplicationUsers).Returns(emptyUsers);
        _mockContext.Setup(c => c.Roles).Returns(roles);
        _mockContext.Setup(c => c.Add(It.IsAny<object>())).Callback<object>(entity =>
        {
            capturedObjects.Add(entity);
            if (entity is ApplicationUser user)
            {
                user.UserId = 1;
                capturedUser = user;
            }
        });
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "mobile_jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginMobileCommand { IdToken = "valid_id_token" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
        Assert.Equal("mobile_jwt_token", result.Token);
        Assert.Contains("Customer", result.Roles);
        Assert.True(result.RequiresPassword); // New Google user needs password
        
        // Verify user, role, and profile were created
        Assert.Contains(capturedObjects, o => o is ApplicationUser);
        Assert.Contains(capturedObjects, o => o is UserRole);
        Assert.Contains(capturedObjects, o => o is UserProfile);
        
        // Verify default profile values for mobile
        var profile = capturedObjects.OfType<UserProfile>().First();
        Assert.Equal(170, profile.HeightCm); // Mobile default
        Assert.Equal(70, profile.WeightKg); // Mobile default
    }

    [Fact]
    public async Task Handle_ExistingUser_AllowsLoginAndReturnsToken()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "existing@test.com",
            Name = "Existing User"
        };

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "existing@test.com",
            PasswordHash = "hashed_password",
            Profile = new UserProfile
            {
                UserId = 1,
                FullName = "Existing User",
                Gender = "Male",
                HeightCm = 180,
                WeightKg = 75
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
                                Permission = new Permission { Id = 1, PermissionCode = "VIEW_PROFILE" }
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

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "mobile_jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string> { "VIEW_PROFILE" }
            });

        var command = new GoogleLoginMobileCommand { IdToken = "valid_id_token" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("existing@test.com", result.Email);
        Assert.Equal("mobile_jwt_token", result.Token);
        Assert.False(result.RequiresPassword); // Existing user has password
        Assert.Contains("Customer", result.Roles);
        Assert.Contains("VIEW_PROFILE", result.Permissions);
    }

    [Fact]
    public async Task Handle_ExistingGoogleUser_AllowsRelogin()
    {
        // Arrange - User created via Google previously (no password)
        var googleUser = new GoogleUserInfo
        {
            Email = "googleuser@test.com",
            Name = "Google User"
        };

        var existingUser = new ApplicationUser
        {
            UserId = 1,
            Email = "googleuser@test.com",
            PasswordHash = "", // No password - Google OAuth user
            Profile = new UserProfile
            {
                UserId = 1,
                FullName = "Google User",
                Gender = "Unknown"
            },
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

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "mobile_jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginMobileCommand { IdToken = "valid_id_token" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("googleuser@test.com", result.Email);
        Assert.True(result.RequiresPassword); // Still needs password for non-Google login
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

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "mobile_jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginMobileCommand { IdToken = "valid_id_token" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(existingUser.LastLoginAt); // Verify LastLoginAt was updated
        Assert.True(existingUser.LastLoginAt.Value >= DateTime.UtcNow.AddSeconds(-5)); // Within last 5 seconds
    }

    [Fact]
    public async Task Handle_NewUser_DefaultProfileValues()
    {
        // Arrange
        var googleUser = new GoogleUserInfo
        {
            Email = "newuser@test.com",
            Name = "New User"
        };

        var emptyUsers = new List<ApplicationUser>().AsQueryable().BuildMock();
        var customerRole = new Role { Id = 2, RoleName = "Customer" };
        var roles = new List<Role> { customerRole }.AsQueryable().BuildMock();

        UserProfile? capturedProfile = null;

        _mockContext.Setup(c => c.ApplicationUsers).Returns(emptyUsers);
        _mockContext.Setup(c => c.Roles).Returns(roles);
        _mockContext.Setup(c => c.Add(It.IsAny<object>())).Callback<object>(entity =>
        {
            if (entity is ApplicationUser user)
            {
                user.UserId = 1;
            }
            else if (entity is UserProfile profile)
            {
                capturedProfile = profile;
            }
        });
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockGoogleAuthService.Setup(g => g.VerifyIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(googleUser);

        _mockJwtTokenService.Setup(j => j.GenerateTokenAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "mobile_jwt_token",
                ExpiresIn = 3600,
                Roles = new List<string> { "Customer" },
                Permissions = new List<string>()
            });

        var command = new GoogleLoginMobileCommand { IdToken = "valid_id_token" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedProfile);
        Assert.Equal("New User", capturedProfile.FullName);
        Assert.Equal("Unknown", capturedProfile.Gender);
        Assert.Equal(170, capturedProfile.HeightCm); // Mobile default
        Assert.Equal(70, capturedProfile.WeightKg); // Mobile default
        Assert.Equal("Moderate", capturedProfile.ActivityLevel);
    }
}

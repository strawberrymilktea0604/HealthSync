using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Handlers;
using HealthSync.Domain.Entities;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class GoogleLoginMobileCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly GoogleLoginMobileCommandHandler _handler;

    public GoogleLoginMobileCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new GoogleLoginMobileCommandHandler(
            _contextMock.Object,
            _googleAuthServiceMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var command = new GoogleLoginMobileCommand { IdToken = "invalid_token" };
        _googleAuthServiceMock.Setup(x => x.VerifyIdTokenAsync("invalid_token"))
            .ReturnsAsync((GoogleUserInfo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsAdmin()
    {
        // Arrange
        var command = new GoogleLoginMobileCommand { IdToken = "valid_token" };
        var googleUser = new GoogleUserInfo { Email = "admin@example.com", Name = "Admin" };
        _googleAuthServiceMock.Setup(x => x.VerifyIdTokenAsync("valid_token"))
            .ReturnsAsync(googleUser);

        var adminRole = new Role { RoleName = "Admin" };
        var adminUser = new ApplicationUser
        {
            Email = "admin@example.com",
            UserRoles = new List<UserRole> { new UserRole { Role = adminRole } }
        };

        var usersList = new List<ApplicationUser> { adminUser };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.ApplicationUsers).Returns(usersMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Admin", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenUserNew()
    {
        // Arrange
        var command = new GoogleLoginMobileCommand { IdToken = "valid_token" };
        var googleUser = new GoogleUserInfo { Email = "new@example.com", Name = "New User" };
        _googleAuthServiceMock.Setup(x => x.VerifyIdTokenAsync("valid_token"))
            .ReturnsAsync(googleUser);

        // Empty user list
        var usersList = new List<ApplicationUser>();
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.ApplicationUsers).Returns(usersMock.Object);

        // Roles
        var customerRole = new Role { Id = 1, RoleName = "Customer" };
        var rolesList = new List<Role> { customerRole };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Roles).Returns(rolesMock.Object);

        // Capture added user
        ApplicationUser? capturedUser = null;
        _contextMock.Setup(x => x.Add(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u => 
            {
                u.UserId = 1;
                u.UserRoles = new List<UserRole> { new UserRole { Role = customerRole } }; // Simulate role assignment logic effect
                capturedUser = u; 
            });

        // Mock Token Service
        _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto { AccessToken = "jwt", Roles = new List<string> { "Customer" } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        _contextMock.Verify(x => x.Add(It.IsAny<ApplicationUser>()), Times.Once); // User
        _contextMock.Verify(x => x.Add(It.IsAny<UserRole>()), Times.Once); // Role
        _contextMock.Verify(x => x.Add(It.IsAny<UserProfile>()), Times.Once); // Profile
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task Handle_ShouldLogin_WhenUserExists()
    {
        // Arrange
        var command = new GoogleLoginMobileCommand { IdToken = "valid_token" };
        var googleUser = new GoogleUserInfo { Email = "existing@example.com", Name = "Existing" };
        _googleAuthServiceMock.Setup(x => x.VerifyIdTokenAsync("valid_token"))
            .ReturnsAsync(googleUser);

        // Existing user
        var customerRole = new Role { RoleName = "Customer" };
        var existingUser = new ApplicationUser
        {
            UserId = 10,
            Email = "existing@example.com",
            UserRoles = new List<UserRole> { new UserRole { Role = customerRole } },
            Profile = new UserProfile { FullName = "Existing", WeightKg = 50, HeightCm = 150, ActivityLevel = "Moderate", Gender = "Male", Dob = DateTime.Now }
        };

        var usersList = new List<ApplicationUser> { existingUser };
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.ApplicationUsers).Returns(usersMock.Object);

         // Mock Token Service
        _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto { AccessToken = "jwt", Roles = new List<string> { "Customer" } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.UserId);
        _contextMock.Verify(x => x.Add(It.IsAny<ApplicationUser>()), Times.Never);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // Update LastLogin
    }
}

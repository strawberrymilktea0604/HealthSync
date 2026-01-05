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
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly GoogleLoginWebCommandHandler _handler;

    public GoogleLoginWebCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new GoogleLoginWebCommandHandler(
            _contextMock.Object,
            _googleAuthServiceMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenCodeInvalid()
    {
        // Arrange
        var command = new GoogleLoginWebCommand { Code = "invalid_code" };
        _googleAuthServiceMock.Setup(x => x.ProcessCallbackAsync("invalid_code"))
            .ReturnsAsync((GoogleUserInfo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsAdmin()
    {
        // Arrange
        var command = new GoogleLoginWebCommand { Code = "valid_code" };
        var googleUser = new GoogleUserInfo { Email = "admin@example.com", Name = "Admin" };
        _googleAuthServiceMock.Setup(x => x.ProcessCallbackAsync("valid_code"))
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
        var command = new GoogleLoginWebCommand { Code = "valid_code" };
        var googleUser = new GoogleUserInfo { Email = "new@example.com", Name = "New User", Picture = "http://pic" };
        _googleAuthServiceMock.Setup(x => x.ProcessCallbackAsync("valid_code"))
            .ReturnsAsync(googleUser);

        // Mock Users
        var usersList = new List<ApplicationUser>(); // Empty initially
        var usersMock = usersList.AsQueryable().BuildMockDbSet();
        
        // Setup sequence for ApplicationUsers: Empty first (check), then return Created User
        var createdUser = new ApplicationUser
        { 
            UserId = 1, 
            Email = "new@example.com",
            Profile = new UserProfile { FullName = "New User" }, // Important for second call 
            UserRoles = new List<UserRole> { new UserRole { Role = new Role{ RoleName = "Customer" } } }
        };

        var createdUsersList = new List<ApplicationUser> { createdUser };
        var createdUsersMock = createdUsersList.AsQueryable().BuildMockDbSet();

        // Standard mock behavior: return empty first
        _contextMock.SetupSequence(x => x.ApplicationUsers)
            .Returns(usersMock.Object) // Check existing Admin
            .Returns(usersMock.Object) // Find by email (null)
            .Returns(createdUsersMock.Object); // Reload after creation

        // Roles
        var customerRole = new Role { Id = 1, RoleName = "Customer" };
        var rolesList = new List<Role> { customerRole };
        var rolesMock = rolesList.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Roles).Returns(rolesMock.Object);

        // Mock Add
        _contextMock.Setup(x => x.Add(It.IsAny<UserProfile>())).Callback<UserProfile>(p => 
        {
             // Verify profile props
             Assert.Equal("http://pic", p.AvatarUrl);
        });

        _contextMock.Setup(x => x.Add(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u => u.UserId = 1);

        // Mock Token Service
        _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new TokenDto { AccessToken = "jwt", Roles = new List<string> { "Customer" } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        _contextMock.Verify(x => x.Add(It.IsAny<ApplicationUser>()), Times.Once); // User
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task Handle_ShouldLoginAndUpdatProfile_WhenUserExists()
    {
         // Arrange
        var command = new GoogleLoginWebCommand { Code = "valid_code" };
        var googleUser = new GoogleUserInfo { Email = "existing@example.com", Name = "Existing", Picture = "http://new-pic" };
        _googleAuthServiceMock.Setup(x => x.ProcessCallbackAsync("valid_code"))
            .ReturnsAsync(googleUser);

        // Existing user
        var customerRole = new Role { RoleName = "Customer" };
        var existingUser = new ApplicationUser
        {
            UserId = 10,
            Email = "existing@example.com",
            UserRoles = new List<UserRole> { new UserRole { Role = customerRole } },
            Profile = new UserProfile { FullName = "", WeightKg = 50, HeightCm = 150, AvatarUrl = "" }
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
        
        // Check Profile Update logic
        Assert.Equal("http://new-pic", existingUser.Profile.AvatarUrl);
        Assert.Equal("Existing", existingUser.Profile.FullName); // Should update because old was empty

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(1)); 
    }
}

using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

using Xunit;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Presentation.Tests.Controllers;

public class UserProfileControllerTests
{
    private readonly Mock<IUserProfileRepository> _profileRepositoryMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly UserProfileController _controller;

    public UserProfileControllerTests()
    {
        _profileRepositoryMock = new Mock<IUserProfileRepository>();
        _mediatorMock = new Mock<IMediator>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _controller = new UserProfileController(_profileRepositoryMock.Object, _mediatorMock.Object, _dbContextMock.Object);
    }

    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WithValidProfile()
    {
        // Arrange
        SetupUserClaims(1);
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.0M,
            WeightKg = 70.0M,
            ActivityLevel = "Moderate",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        _profileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(profile);

        var appUser = new ApplicationUser { UserId = 1, AvatarUrl = "https://example.com/avatar.jpg" };
        _dbContextMock.Setup(c => c.ApplicationUsers).Returns(new List<ApplicationUser> { appUser }.AsQueryable().BuildMockDbSet().Object);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserProfileDto>(okResult.Value);
        Assert.Equal(profile.UserId, dto.UserId);
        Assert.Equal(profile.FullName, dto.FullName);
        Assert.Equal(profile.Dob, dto.Dob);
        Assert.Equal(profile.Gender, dto.Gender);
        Assert.Equal(profile.HeightCm, dto.HeightCm);
        Assert.Equal(profile.WeightKg, dto.WeightKg);
        Assert.Equal(profile.ActivityLevel, dto.ActivityLevel);
        Assert.Equal(profile.AvatarUrl, dto.AvatarUrl);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenProfileNotExists()
    {
        // Arrange
        SetupUserClaims(1);
        _profileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync((UserProfile?)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Profile not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WithoutUserIdClaim()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WithInvalidUserIdClaim()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-id")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnOk_WhenUpdateSuccessful()
    {
        // Arrange
        SetupUserClaims(1);
        var existingProfile = new UserProfile
        {
            UserId = 1,
            FullName = "Old Name",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 170.0M,
            WeightKg = 65.0M,
            ActivityLevel = "Low",
            AvatarUrl = null
        };

        var updateDto = new UpdateUserProfileDto
        {
            FullName = "New Name",
            Dob = new DateTime(1992, 5, 15),
            Gender = "Female",
            HeightCm = 165.0M,
            WeightKg = 60.0M,
            ActivityLevel = "High",
            AvatarUrl = "https://example.com/new-avatar.jpg"
        };

        _profileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(existingProfile);
        _profileRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Profile updated successfully", okResult.Value);

        // Verify the profile was updated correctly
        _profileRepositoryMock.Verify(r => r.UpdateAsync(It.Is<UserProfile>(p =>
            p.UserId == 1 &&
            p.FullName == updateDto.FullName &&
            p.Dob == updateDto.Dob &&
            p.Gender == updateDto.Gender &&
            p.HeightCm == updateDto.HeightCm &&
            p.WeightKg == updateDto.WeightKg &&
            p.ActivityLevel == updateDto.ActivityLevel &&
            p.AvatarUrl == updateDto.AvatarUrl
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnNotFound_WhenProfileNotExists()
    {
        // Arrange
        SetupUserClaims(1);
        _profileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync((UserProfile?)null);

        var updateDto = new UpdateUserProfileDto
        {
            FullName = "New Name"
        };

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Profile not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnUnauthorized_WithoutUserIdClaim()
    {
        // Arrange
        var updateDto = new UpdateUserProfileDto
        {
            FullName = "New Name"
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
using HealthSync.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Services;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_WhenUserIsAuthenticated_ReturnsUserId()
    {
        // Arrange
        var userId = "123";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.UserId;

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void UserId_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Email_WhenUserIsAuthenticated_ReturnsEmail()
    {
        // Arrange
        var email = "test@example.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.Email;

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void Email_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.Email;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Email_WhenUserIsNull_ReturnsNull()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.Email;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WhenHttpContextIsNull_ReturnsFalse()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WhenUserIsNull_ReturnsFalse()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WhenIdentityIsNull_ReturnsFalse()
    {
        // Arrange
        var mockIdentity = new Mock<ClaimsIdentity>();
        mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
        
        var claimsPrincipal = new ClaimsPrincipal(mockIdentity.Object);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var service = new CurrentUserService(mockHttpContextAccessor.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        Assert.False(result);
    }
}

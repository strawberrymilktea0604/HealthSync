using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using HealthSync.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthSync.Presentation.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _configMock = new Mock<IConfiguration>();
        _controller = new AuthController(_mediatorMock.Object, _loggerMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WithValidCredentials()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var expectedResponse = new AuthResponse
        {
            UserId = 1,
            Email = "user@example.com",
            Token = "jwt-token",
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.UserId, response.UserId);
        Assert.Equal(expectedResponse.Email, response.Email);
        Assert.Equal(expectedResponse.Token, response.Token);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WithInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrongpassword"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var error = unauthorizedResult.Value?.GetType().GetProperty("Error")?.GetValue(unauthorizedResult.Value);
        Assert.Equal("Sai email hoặc mật khẩu!", error);
    }

    [Fact]
    public async Task Login_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "password123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var error = statusCodeResult.Value?.GetType().GetProperty("Error")?.GetValue(statusCodeResult.Value);
        Assert.Equal("Lỗi server nội bộ", error);
    }

    [Fact]
    public async Task SendVerificationCode_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SendVerificationCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.SendVerificationCode(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("Mã xác thực đã được gửi đến email của bạn!", message);
    }

    [Fact]
    public async Task SendVerificationCode_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SendVerificationCodeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _controller.SendVerificationCode(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var error = statusCodeResult.Value?.GetType().GetProperty("Error")?.GetValue(statusCodeResult.Value);
        Assert.Equal("Lỗi server nội bộ", error);
    }

    [Fact]
    public void GetFrontendUrl_ShouldReturnConfiguredUrl()
    {
        // Arrange
        _configMock.Setup(c => c["FRONTEND_URL"]).Returns("https://myapp.com");

        // Act
        var url = _controller.GetType().GetMethod("GetFrontendUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_controller, null);

        // Assert
        Assert.Equal("https://myapp.com", url);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WithValidData()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        var expectedResponse = new AuthResponse
        {
            UserId = 1,
            Email = "newuser@example.com",
            Token = "jwt-token",
            FullName = "New User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.UserId, response.UserId);
        Assert.Equal(expectedResponse.Email, response.Email);
        Assert.Equal(expectedResponse.Token, response.Token);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WithInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value?.GetType().GetProperty("Error")?.GetValue(badRequestResult.Value);
        Assert.Equal("Email already exists", error);
    }

    [Fact]
    public async Task Register_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "password123",
            VerificationCode = "123456"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var error = statusCodeResult.Value?.GetType().GetProperty("Error")?.GetValue(statusCodeResult.Value);
        Assert.Equal("Lỗi server nội bộ", error);
    }

    [Fact]
    public async Task RegisterAdmin_ShouldReturnOk_WithValidData()
    {
        // Arrange
        var request = new RegisterAdminRequest
        {
            Email = "admin@example.com",
            Password = "admin123",
            FullName = "Admin User"
        };

        var expectedResponse = new AuthResponse
        {
            UserId = 1,
            Email = "admin@example.com",
            Token = "jwt-token",
            FullName = "Admin User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterAdminCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RegisterAdmin(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Email, response.Email);
    }

    [Fact]
    public async Task VerifyCode_ShouldReturnOk_WithValidCode()
    {
        // Arrange
        var request = new VerifyCodeRequest
        {
            Email = "user@example.com",
            Code = "123456"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyCode(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("Mã xác thực hợp lệ!", message);
    }

    [Fact]
    public async Task VerifyCode_ShouldReturnBadRequest_WithInvalidCode()
    {
        // Arrange
        var request = new VerifyCodeRequest
        {
            Email = "user@example.com",
            Code = "wrong"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyEmailCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.VerifyCode(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value?.GetType().GetProperty("Error")?.GetValue(badRequestResult.Value);
        Assert.Equal("Mã xác thực không hợp lệ hoặc đã hết hạn", error);
    }

    [Fact]
    public async Task SetPassword_ShouldReturnOk_WithValidData()
    {
        // Arrange
        var request = new SetPasswordRequest
        {
            UserId = 1,
            Password = "newpassword123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _controller.SetPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("Mật khẩu đã được đặt thành công!", message);
    }

    [Fact]
    public async Task GoogleLoginMobile_ShouldReturnOk_WithValidToken()
    {
        // Arrange
        var request = new GoogleLoginMobileRequest
        {
            IdToken = "valid-id-token"
        };

        var expectedResponse = new AuthResponse
        {
            UserId = 1,
            Email = "google@example.com",
            Token = "jwt-token",
            FullName = "Google User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GoogleLoginMobileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GoogleLoginMobile(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Email, response.Email);
    }

    [Fact]
    public async Task GetGoogleAndroidClientId_ShouldReturnOk()
    {
        // Arrange
        var expectedClientId = "android-client-id-123";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoogleAndroidClientIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedClientId);

        // Act
        var result = await _controller.GetGoogleAndroidClientId();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var clientId = okResult.Value?.GetType().GetProperty("ClientId")?.GetValue(okResult.Value);
        Assert.Equal(expectedClientId, clientId);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_WithValidEmail()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.NotNull(message);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnOk_WithValidData()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "reset-token-123",
            NewPassword = "newpassword123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("Password reset successfully", message);
    }

    [Fact]
    public async Task VerifyResetOtp_ShouldReturnOk_WithValidOtp()
    {
        // Arrange
        var request = new VerifyOtpRequest
        {
            Email = "user@example.com",
            Otp = "123456"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyResetOtpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyResetOtpResponse { Success = true, Message = "OTP verified", Token = "reset-token" });

        // Act
        var result = await _controller.VerifyResetOtp(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("OTP verified", message);
    }

    [Fact]
    public async Task VerifyResetOtp_ShouldReturnBadRequest_WithInvalidOtp()
    {
        // Arrange
        var request = new VerifyOtpRequest
        {
            Email = "user@example.com",
            Otp = "wrong"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyResetOtpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifyResetOtpResponse { Success = false, Message = "Invalid OTP" });

        // Act
        var result = await _controller.VerifyResetOtp(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value?.GetType().GetProperty("Error")?.GetValue(badRequestResult.Value);
        Assert.Equal("Invalid OTP", error);
    }

    [Fact]
    public async Task ResendResetOtp_ShouldReturnOk_WithValidEmail()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResendResetOtpCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _controller.ResendResetOtp(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value);
        Assert.Equal("OTP resent", message);
    }

    [Fact]
    public async Task RegisterAdmin_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new RegisterAdminRequest
        {
            Email = "admin@example.com",
            Password = "Admin@123",
            VerificationCode = "123456",
            FullName = "Admin User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterAdminCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.RegisterAdmin(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task RegisterAdmin_ShouldReturnBadRequest_WithInvalidOperation()
    {
        // Arrange
        var request = new RegisterAdminRequest
        {
            Email = "admin@example.com",
            Password = "weak",
            VerificationCode = "123456",
            FullName = "Admin User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterAdminCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid verification code"));

        // Act
        var result = await _controller.RegisterAdmin(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenInvalidOperation()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "invalid-token",
            NewPassword = "newpass123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value?.GetType().GetProperty("Error")?.GetValue(badRequestResult.Value);
        Assert.Equal("Invalid token", error);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "token",
            NewPassword = "newpass123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task SetPassword_ShouldReturnBadRequest_WhenInvalidOperation()
    {
        // Arrange
        var request = new SetPasswordRequest
        {
            UserId = 1,
            Password = "weak"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Password too weak"));

        // Act
        var result = await _controller.SetPassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value?.GetType().GetProperty("Error")?.GetValue(badRequestResult.Value);
        Assert.Equal("Password too weak", error);
    }

    [Fact]
    public async Task SetPassword_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new SetPasswordRequest
        {
            UserId = 1,
            Password = "NewPass@123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.SetPassword(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GoogleLoginMobile_ShouldReturnUnauthorized_WhenUnauthorizedAccess()
    {
        // Arrange
        var request = new GoogleLoginMobileRequest
        {
            IdToken = "invalid-token"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GoogleLoginMobileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid Google token"));

        // Act
        var result = await _controller.GoogleLoginMobile(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var error = unauthorizedResult.Value?.GetType().GetProperty("Error")?.GetValue(unauthorizedResult.Value);
        Assert.Equal("Invalid Google token", error);
    }

    [Fact]
    public async Task GoogleLoginMobile_ShouldReturnInternalServerError_WhenUnexpectedException()
    {
        // Arrange
        var request = new GoogleLoginMobileRequest
        {
            IdToken = "valid-token"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GoogleLoginMobileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Google API error"));

        // Act
        var result = await _controller.GoogleLoginMobile(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetGoogleAndroidClientId_ShouldReturnNotFound_WhenNotConfigured()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoogleAndroidClientIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _controller.GetGoogleAndroidClientId();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetGoogleAndroidClientId_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoogleAndroidClientIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Configuration error"));

        // Act
        var result = await _controller.GetGoogleAndroidClientId();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task VerifyResetOtp_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        var request = new VerifyOtpRequest
        {
            Email = "user@example.com",
            Otp = "123456"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyResetOtpCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.VerifyResetOtp(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ResendResetOtp_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        var request = new SendVerificationCodeRequest
        {
            Email = "user@example.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResendResetOtpCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _controller.ResendResetOtp(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public void GoogleLoginWeb_ShouldReturnRedirect_WithValidState()
    {
        // Arrange
        var authUrl = "https://accounts.google.com/o/oauth2/auth?client_id=test";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoogleAuthUrlQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authUrl);

        // Act
        var result = _controller.GoogleLoginWeb("test-state");

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(authUrl, redirectResult.Url);
    }

    [Fact]
    public void GoogleLoginWeb_ShouldReturnInternalServerError_WhenException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetGoogleAuthUrlQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Google OAuth error"));

        // Act
        var result = _controller.GoogleLoginWeb("test-state");

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GoogleCallback_ShouldReturnRedirect_WithValidCode()
    {
        // Arrange
        var code = "test-auth-code";
        var authResponse = new AuthResponse
        {
            Token = "test-token",
            UserId = 1,
            Email = "user@example.com",
            Role = "Customer"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GoogleLoginWebCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.GoogleCallback(code);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Contains("token=", redirectResult.Url);
        Assert.Contains("userId=1", redirectResult.Url);
    }

    [Fact]
    public async Task GoogleCallback_ShouldReturnBadRequest_WithMissingCode()
    {
        // Act
        var result = await _controller.GoogleCallback("");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GoogleCallback_ShouldReturnRedirect_WhenException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GoogleLoginWebCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Google callback error"));

        // Act
        var result = await _controller.GoogleCallback("test-code");

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Contains("error=", redirectResult.Url);
    }
}

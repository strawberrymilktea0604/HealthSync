using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request)
    {
        try
        {
            var command = new RegisterAdminCommand
            {
                Email = request.Email,
                Password = request.Password,
                VerificationCode = request.VerificationCode,
                FullName = request.FullName
            };

            var authResponse = await _mediator.Send(command);
            return Ok(authResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering admin");
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                VerificationCode = request.VerificationCode
            };

            var authResponse = await _mediator.Send(command);
            return Ok(authResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var query = new LoginQuery
            {
                Email = request.Email,
                Password = request.Password
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Error = "Sai email hoặc mật khẩu!" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("send-verification-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequest request)
    {
        try
        {
            var command = new SendVerificationCodeCommand
            {
                Email = request.Email
            };

            await _mediator.Send(command);
            return Ok(new { Message = "Mã xác thực đã được gửi đến email của bạn!" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { Error = "Email và mã xác thực không được để trống" });
            }

            var command = new VerifyEmailCodeCommand
            {
                Email = request.Email,
                Code = request.Code
            };

            var isVerified = await _mediator.Send(command);
            if (isVerified)
            {
                return Ok(new { Message = "Mã xác thực hợp lệ!", Success = true });
            }
            else
            {
                return BadRequest(new { Error = "Mã xác thực không hợp lệ hoặc đã hết hạn", Success = false });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email code for {Email}", request?.Email);
            return StatusCode(500, new { Error = "Lỗi server nội bộ", Success = false });
        }
    }

    [HttpGet("google/web")]
    public IActionResult GoogleLoginWeb([FromQuery] string state = "")
    {
        try
        {
            var command = new GetGoogleAuthUrlQuery { State = state };
            var authUrl = _mediator.Send(command).Result;
            return Redirect(authUrl);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string? state = null)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { Error = "Authorization code is missing" });
            }

            var command = new GoogleLoginWebCommand
            {
                Code = code,
                State = state ?? string.Empty
            };
            
            var response = await _mediator.Send(command);

            // Redirect to frontend with token
            var frontendUrl = "http://localhost:5173"; // Frontend port
            var redirectUrl = $"{frontendUrl}/google/callback?" +
                $"token={Uri.EscapeDataString(response.Token)}&" +
                $"userId={response.UserId}&" +
                $"email={Uri.EscapeDataString(response.Email)}&" +
                $"fullName={Uri.EscapeDataString(response.FullName)}&" +
                $"role={Uri.EscapeDataString(response.Role)}&" +
                $"requiresPassword={response.RequiresPassword.ToString().ToLower()}&" +
                $"isProfileComplete={response.IsProfileComplete.ToString().ToLower()}&" +
                $"expiresAt={Uri.EscapeDataString(response.ExpiresAt.ToString("O"))}";

            return Redirect(redirectUrl);
        }
        catch (UnauthorizedAccessException ex)
        {
            var frontendUrl = "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(ex.Message)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GoogleCallback: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            var frontendUrl = "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Invalid callback parameters")}");
        }
    }

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
    {
        try
        {
            var command = new SetPasswordCommand
            {
                UserId = request.UserId,
                Password = request.Password
            };

            await _mediator.Send(command);
            return Ok(new { Message = "Mật khẩu đã được đặt thành công!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("google/mobile")]
    public async Task<IActionResult> GoogleLoginMobile([FromBody] GoogleLoginMobileRequest request)
    {
        try
        {
            var command = new GoogleLoginMobileCommand
            {
                IdToken = request.IdToken
            };

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpGet("google/android-client-id")]
    public async Task<IActionResult> GetGoogleAndroidClientId()
    {
        try
        {
            var query = new GetGoogleAndroidClientIdQuery();
            var androidClientId = await _mediator.Send(query);
            
            if (string.IsNullOrEmpty(androidClientId))
            {
                return NotFound(new { Error = "Android Client ID not configured" });
            }

            return Ok(new { ClientId = androidClientId });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] SendVerificationCodeRequest request)
    {
        try
        {
            var command = new ForgotPasswordCommand
            {
                Email = request.Email
            };

            await _mediator.Send(command);
            return Ok(new { Message = "If the email exists, a reset link has been sent." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var command = new ResetPasswordCommand
            {
                Token = request.Token,
                NewPassword = request.NewPassword
            };

            await _mediator.Send(command);
            return Ok(new { Message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("verify-reset-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var command = new VerifyResetOtpCommand
            {
                Email = request.Email,
                Otp = request.Otp
            };

            var isValid = await _mediator.Send(command);
            if (isValid)
            {
                return Ok(new { Message = "OTP verified" });
            }
            else
            {
                return BadRequest(new { Error = "Invalid OTP" });
            }
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }

    [HttpPost("resend-reset-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendResetOtp([FromBody] SendVerificationCodeRequest request)
    {
        try
        {
            var command = new ResendResetOtpCommand
            {
                Email = request.Email
            };
            await _mediator.Send(command);
            return Ok(new { Message = "OTP resent" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = "Lỗi server nội bộ" });
        }
    }
}

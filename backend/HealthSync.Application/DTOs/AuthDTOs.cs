namespace HealthSync.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class SendVerificationCodeRequest
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyCodeRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class GoogleLoginMobileRequest
{
    public string IdToken { get; set; } = string.Empty;
}
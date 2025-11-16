namespace HealthSync.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
    public bool RequiresPassword { get; set; } = false; // True if user needs to set password (first-time Google login)
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

public class SetPasswordRequest
{
    public int UserId { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class RegisterAdminRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
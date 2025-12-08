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
    public string Role { get; set; } = string.Empty; // Primary role for backward compatibility
    public List<string> Roles { get; set; } = new(); // All roles assigned to user
    public List<string> Permissions { get; set; } = new(); // All permissions user has
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool RequiresPassword { get; set; } = false; // True if user needs to set password (first-time Google login)
    public bool IsProfileComplete { get; set; } = false; // True if user profile is fully filled
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

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ResendOtpRequest
{
    // Có thể thêm email nếu cần
}
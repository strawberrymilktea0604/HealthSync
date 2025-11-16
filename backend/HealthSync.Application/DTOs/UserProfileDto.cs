namespace HealthSync.Application.DTOs;

// DTO for GET profile response (includes UserId)
public class UserProfileDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }
}

// DTO for PUT profile request (NO UserId - taken from token)
public class UpdateUserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }
}
namespace HealthSync.Application.DTOs;

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
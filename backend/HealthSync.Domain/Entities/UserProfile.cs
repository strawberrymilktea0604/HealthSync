namespace HealthSync.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
namespace HealthSync.Domain.Entities;

public class UserProfile
{
    public Guid UserProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal InitialHeightCm { get; set; }
    public decimal InitialWeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
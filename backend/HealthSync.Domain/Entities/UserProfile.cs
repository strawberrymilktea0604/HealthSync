namespace HealthSync.Domain.Entities;

public class UserProfile
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime Dob { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal HeightCm { get; set; }
    public decimal WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "Moderate";
    public string? AvatarUrl { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;

    // Method to check if profile is complete
    public bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(FullName) &&
               !string.IsNullOrWhiteSpace(Gender) &&
               Gender != "Unknown" &&
               HeightCm > 0 &&
               WeightKg > 0 &&
               Dob < DateTime.UtcNow.AddYears(-10); // At least 10 years old
    }
}
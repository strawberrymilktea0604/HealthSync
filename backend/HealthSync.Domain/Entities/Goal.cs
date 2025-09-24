namespace HealthSync.Domain.Entities;

public class Goal
{
    public Guid GoalId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., "weight_loss", "muscle_gain"
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "active"; // active, completed, paused
    public string? Notes { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ProgressRecord> ProgressRecords { get; set; } = new List<ProgressRecord>();
}
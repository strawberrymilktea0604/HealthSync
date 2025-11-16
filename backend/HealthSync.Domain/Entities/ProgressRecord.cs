namespace HealthSync.Domain.Entities;

public class ProgressRecord
{
    public int ProgressRecordId { get; set; }
    public int GoalId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal Value { get; set; }
    public string? Notes { get; set; }
    public decimal WeightKg { get; set; }
    public decimal WaistCm { get; set; }

    // Navigation property
    public Goal Goal { get; set; } = null!;
}
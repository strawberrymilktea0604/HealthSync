namespace HealthSync.Domain.Entities;

public class ProgressRecord
{
    public Guid ProgressRecordId { get; set; }
    public Guid GoalId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal RecordValue { get; set; }
    public decimal? Weight { get; set; }
    public decimal? WaistCm { get; set; }

    // Navigation property
    public Goal Goal { get; set; } = null!;
}
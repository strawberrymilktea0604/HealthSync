namespace HealthSync.Application.DTOs;

public class CreateGoalRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class GoalResponse
{
    public int GoalId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<ProgressRecordResponse> ProgressRecords { get; set; } = new List<ProgressRecordResponse>();
}

public class GetGoalsResponse
{
    public List<GoalResponse> Goals { get; set; } = new List<GoalResponse>();
}

public class AddProgressRequest
{
    public DateTime RecordDate { get; set; }
    public decimal Value { get; set; }
    public string? Notes { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
}

public class ProgressRecordResponse
{
    public int ProgressRecordId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal Value { get; set; }
    public string? Notes { get; set; }
    public decimal WeightKg { get; set; }
    public decimal WaistCm { get; set; }
}

public class AddProgressResponse
{
    public ProgressRecordResponse ProgressRecord { get; set; } = null!;
}
namespace HealthSync.Application.DTOs;

public class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid MessageId { get; set; }
}

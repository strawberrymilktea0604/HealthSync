namespace HealthSync.Domain.Entities;

public class ChatMessage
{
    public Guid ChatMessageId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ContextData { get; set; } // JSON snapshot of user data at time of request
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
}

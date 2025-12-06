namespace HealthSync.Domain.Interfaces;

public interface IAiChatService
{
    Task<string> GetHealthAdviceAsync(string userContextData, string userQuestion, CancellationToken cancellationToken = default);
}

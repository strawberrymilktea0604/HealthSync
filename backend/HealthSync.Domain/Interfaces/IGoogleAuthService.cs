using System.Threading.Tasks;

namespace HealthSync.Domain.Interfaces;

public interface IGoogleAuthService
{
    Task<string> GetAuthorizationUrl(string state);
    Task<GoogleUserInfo?> ProcessCallbackAsync(string code, string state);
    Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken);
}

public class GoogleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
}
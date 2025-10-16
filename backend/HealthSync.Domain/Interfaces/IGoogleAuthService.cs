using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HealthSync.Domain.Interfaces;

public interface IGoogleAuthService
{
    Task<string> GetAuthorizationUrl(string state);
    Task<GoogleUserInfo?> ProcessCallbackAsync(string code);
    Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken);
    Task<string> GetAndroidClientIdAsync();
}

public class GoogleUserInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("picture")]
    public string Picture { get; set; } = string.Empty;
}
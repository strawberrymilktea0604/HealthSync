using HealthSync.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace HealthSync.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GoogleAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public Task<string> GetAuthorizationUrl(string state)
    {
        var clientId = _configuration["GoogleOAuth:ClientId"] ?? "";
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"] ?? "http://localhost:5274/api/auth/google/callback";

        var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                  $"client_id={HttpUtility.UrlEncode(clientId)}&" +
                  $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&" +
                  $"response_type=code&" +
                  $"scope=openid%20email%20profile&" +
                  $"state={HttpUtility.UrlEncode(state)}";

        return Task.FromResult(url);
    }

    public async Task<GoogleUserInfo?> ProcessCallbackAsync(string code, string state)
    {
        var clientId = _configuration["GoogleOAuth:ClientId"] ?? "";
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"] ?? "";
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"] ?? "http://localhost:5274/api/auth/google/callback";

        // Exchange code for access token
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            })
        };

        var tokenResponse = await _httpClient.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode) return null;

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
        var accessToken = tokenData.GetProperty("access_token").GetString();

        // Get user info
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo")
        {
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        };

        var userInfoResponse = await _httpClient.SendAsync(userInfoRequest);
        if (!userInfoResponse.IsSuccessStatusCode) return null;

        var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);

        return userInfo;
    }

    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken)
    {
        // Verify ID token with Google
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get,
            $"https://oauth2.googleapis.com/tokeninfo?id_token={HttpUtility.UrlEncode(idToken)}");

        var verifyResponse = await _httpClient.SendAsync(verifyRequest);
        if (!verifyResponse.IsSuccessStatusCode) return null;

        var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<JsonElement>(verifyContent);

        if (tokenInfo.TryGetProperty("error", out _)) return null;

        var userInfo = new GoogleUserInfo
        {
            Id = tokenInfo.GetProperty("sub").GetString() ?? "",
            Email = tokenInfo.GetProperty("email").GetString() ?? "",
            Name = tokenInfo.GetProperty("name").GetString() ?? "",
            Picture = tokenInfo.GetProperty("picture").GetString() ?? ""
        };

        return userInfo;
    }
}
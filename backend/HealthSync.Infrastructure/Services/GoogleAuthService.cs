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
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"] ?? _configuration["GOOGLE_REDIRECT_URI"];
        var authUri = _configuration["GOOGLE_AUTH_URI"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(authUri))
        {
            throw new InvalidOperationException("Google OAuth configuration is missing");
        }

        var url = $"{authUri}?" +
                  $"client_id={HttpUtility.UrlEncode(clientId)}&" +
                  $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&" +
                  $"response_type=code&" +
                  $"scope=openid%20email%20profile&" +
                  $"state={HttpUtility.UrlEncode(state)}";

        return Task.FromResult(url);
    }

    public async Task<GoogleUserInfo?> ProcessCallbackAsync(string code)
    {
        try
        {
            var clientId = _configuration["GoogleOAuth:ClientId"] ?? "";
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"] ?? "";
            var redirectUri = _configuration["GoogleOAuth:RedirectUri"] ?? _configuration["GOOGLE_REDIRECT_URI"];
            var tokenUri = _configuration["GOOGLE_TOKEN_URI"];

            if (string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(tokenUri))
            {
                throw new InvalidOperationException("Google OAuth configuration is missing");
            }

            // Exchange code for access token
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUri)
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
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Token request failed: {errorContent}");
                return null;
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Access token is null or empty");
                return null;
            }

            // Get user info
            var userInfoUri = _configuration["GOOGLE_USERINFO_URI"];

            if (string.IsNullOrEmpty(userInfoUri))
            {
                throw new InvalidOperationException("Google OAuth configuration is missing");
            }

            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, userInfoUri)
            {
                Headers = { { "Authorization", $"Bearer {accessToken}" } }
            };

            var userInfoResponse = await _httpClient.SendAsync(userInfoRequest);
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"UserInfo request failed: {errorContent}");
                return null;
            }

            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"UserInfo response: {userInfoContent}");
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent, options);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                Console.WriteLine("Failed to deserialize user info or email is empty");
                return null;
            }

            return userInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessCallbackAsync: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken)
    {
        var tokenInfoUri = _configuration["GOOGLE_TOKENINFO_URI"];

        if (string.IsNullOrEmpty(tokenInfoUri))
        {
            throw new InvalidOperationException("Google OAuth configuration is missing");
        }

        // Verify ID token with Google
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get,
            $"{tokenInfoUri}?id_token={HttpUtility.UrlEncode(idToken)}");

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

    public Task<string> GetAndroidClientIdAsync()
    {
        var androidClientId = _configuration["GoogleOAuth:AndroidClientId"] ?? "";
        return Task.FromResult(androidClientId);
    }
}
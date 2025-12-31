using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HealthSync.IntegrationTests.Helpers
{
    public static class TestTokenGenerator
    {
        public static string GenerateJwtToken()
        {
            // Lấy key từ env
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            
            // Fallback key: Khớp với CustomWebApplicationFactory.cs
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 16)
            {
                secretKey = "ThisIsATestSecretKeyForIntegrationTestingPurposesOnly123456";
            }

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "1"), // userId
                new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", "1"), // userId
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim("Permission", "USER_READ"),
                new Claim("Permission", "EXERCISE_READ"),
                new Claim("Permission", "FOOD_READ"),
                new Claim("Permission", "WORKOUT_LOG_READ"),
                new Claim("Permission", "NUTRITION_LOG_READ"),
                new Claim("Permission", "DASHBOARD_VIEW")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                
                // KHỚP VỚI CustomWebApplicationFactory.cs (Test config)
                Issuer = "TestIssuer",        
                Audience = "TestAudience", 
                
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
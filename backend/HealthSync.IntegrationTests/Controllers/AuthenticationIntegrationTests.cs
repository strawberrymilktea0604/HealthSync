using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests.Controllers;

public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthenticationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Helper method to hash password using SHA256 (same as AuthService)
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    [Fact]
    public async Task SendVerificationCode_ShouldReturnOk()
    {
        // Arrange
        var email = $"testuser_{Guid.NewGuid()}@example.com";
        var request = new { Email = email };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/send-verification-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        // Check if response has "Message" property (capital M - matches C# model)
        var hasMessage = content.TryGetProperty("Message", out var messageValue);
        Assert.True(hasMessage, $"Response should have 'Message' property. Actual response: {responseBody}");
        Assert.False(string.IsNullOrEmpty(messageValue.GetString()), "Message value should not be empty");
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/chat/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", content.GetProperty("status").GetString());
        Assert.Equal("HealthSync Chatbot", content.GetProperty("service").GetString());
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnJwtToken()
    {
        // Arrange
        var email = $"logintest_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password); // Use SHA256 hash (same as AuthService)

        // Seed user with Customer role
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                customerRole = new Role { RoleName = "Customer", Description = "Customer role" };
                dbContext.Roles.Add(customerRole);
                await dbContext.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
            await dbContext.SaveChangesAsync();
            
            // Verify user was saved
            var savedUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            Assert.NotNull(savedUser);
            Assert.Equal(passwordHash, savedUser.PasswordHash);
        }

        // Act - Try to login
        var loginRequest = new { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Debug: Check if user still exists in DB when login is called
        using (var scope2 = _factory.Services.CreateScope())
        {
            var dbContext2 = scope2.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var userCheck = await dbContext2.ApplicationUsers
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            Assert.NotNull(userCheck); // User should still exist
            Assert.NotNull(userCheck.PasswordHash); // Password hash should exist
            Assert.NotEmpty(userCheck.UserRoles); // Should have roles
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal(email, authResponse.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = $"wrongpass_{Guid.NewGuid()}@example.com";

        // Register user
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "CorrectPassword@123",
            FullName = "Test User",
            Dob = "1990-01-01",
            Gender = "Male"
        });

        // Act - Login with wrong password
        var loginRequest = new
        {
            Email = email,
            Password = "WrongPassword@123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginAndAuthentication_FullFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        var email = $"fullflow_{Guid.NewGuid()}@example.com";
        var password = "SecurePass@123";
        var passwordHash = HashPassword(password); // Use SHA256 hash

        // Seed user with role
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                customerRole = new Role { RoleName = "Customer", Description = "Customer role" };
                dbContext.Roles.Add(customerRole);
                await dbContext.SaveChangesAsync();
            }

            var user = new ApplicationUser { Email = email, PasswordHash = passwordHash, IsActive = true };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
            await dbContext.SaveChangesAsync();
        }

        // Act: Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse!.Token);
        Assert.Equal(email, authResponse.Email);
    }
}

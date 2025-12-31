using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests.Controllers;

public class GoalsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public GoalsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var loginRequest = new { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        return authResponse!.Token;
    }

    [Fact]
    public async Task CreateGoal_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new 
        { 
            Type = "Weight Loss",
            TargetValue = 65.0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Notes = "Lose weight"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/goals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateGoal_WithAuth_ShouldCreateGoal()
    {
        // Arrange
        var email = $"goaluser_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Seed user
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new 
        { 
            Type = "Weight Loss",
            TargetValue = 65.0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Notes = "Lose 5kg in 30 days"
        };
        var response = await _client.PostAsJsonAsync("/api/goals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetGoals_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/goals");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGoals_WithAuth_ShouldReturnUserGoals()
    {
        // Arrange
        var email = $"goalsuser_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int userId;
        // Seed user and goals
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            userId = user.UserId;

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();
            
            // Add goals
            dbContext.Goals.AddRange(
                new Goal
                {
                    UserId = userId,
                    Type = "Weight Loss",
                    TargetValue = 65,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Notes = "First goal"
                },
                new Goal
                {
                    UserId = userId,
                    Type = "Muscle Gain",
                    TargetValue = 75,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(60),
                    Notes = "Second goal"
                }
            );

            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/goals");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var goalsText = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(goalsText));
    }

    [Fact]
    public async Task AddProgress_WithAuth_ShouldAddProgressToGoal()
    {
        // Arrange
        var email = $"progressuser_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int userId;
        int goalId;
        
        // Seed user and goal
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            userId = user.UserId;

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();
            
            var goal = new Goal
            {
                UserId = userId,
                Type = "Weight Loss",
                TargetValue = 65,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Notes = "Test goal"
            };
            dbContext.Goals.Add(goal);
            await dbContext.SaveChangesAsync();

            goalId = goal.GoalId;
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new 
        { 
            RecordDate = DateTime.UtcNow,
            Value = 68.0,
            WeightKg = 68.0,
            Notes = "Progress update"
        };
        var response = await _client.PostAsJsonAsync($"/api/goals/{goalId}/progress", request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.Created,
            $"Expected OK or Created, but got {response.StatusCode}");
    }

    [Fact]
    public async Task AddProgress_WithInvalidGoalId_ShouldReturnNotFound()
    {
        // Arrange
        var email = $"invalidgoal_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Seed user
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new 
        { 
            RecordDate = DateTime.UtcNow,
            Value = 68.0,
            WeightKg = 68.0,
            Notes = "Progress update"
        };
        var response = await _client.PostAsJsonAsync($"/api/goals/99999/progress", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

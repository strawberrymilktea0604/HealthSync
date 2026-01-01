using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests;

public class SecurityIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SecurityIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task DeleteWorkoutLog_AsOtherUser_ShouldReturnForbidden_AndNotDelete()
    {
        // 1. Arrange: Setup User A (Attacker) and User B (Victim)
        var attackerEmail = $"attacker_{Guid.NewGuid()}@example.com";
        var victimEmail = $"victim_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int victimLogId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            // Create Attacker
            var attacker = new ApplicationUser
            {
                Email = attackerEmail,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(attacker);

            // Create Victim
            var victim = new ApplicationUser
            {
                Email = victimEmail,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(victim);
            
            await dbContext.SaveChangesAsync();

            // Assign Roles
            dbContext.UserRoles.Add(new UserRole { UserId = attacker.UserId, RoleId = customerRole!.Id });
            dbContext.UserRoles.Add(new UserRole { UserId = victim.UserId, RoleId = customerRole.Id });
            await dbContext.SaveChangesAsync();

            // Create Victim's Data (Workout Log)
            var log = new WorkoutLog
            {
                UserId = victim.UserId,
                WorkoutDate = DateTime.UtcNow,
                DurationMin = 60,
                Notes = "Victim's private log"
            };
            dbContext.WorkoutLogs.Add(log);
            await dbContext.SaveChangesAsync();
            
            victimLogId = log.WorkoutLogId;
        }

        // 2. Act: Attacker logs in and tries to delete Victim's log
        var token = await LoginAndGetTokenAsync(attackerEmail, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/workout/workout-logs/{victimLogId}");

        // 3. Assert: 
        // Expect 403 Forbidden (handled by Controller catching UnauthorizedAccessException)
        // Note: If the IDOR protection wasn't there, it might return 204 No Content (Deleted)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Verify data still exists in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var log = await dbContext.WorkoutLogs.FindAsync(victimLogId);
            Assert.NotNull(log);
            Assert.Equal("Victim's private log", log.Notes);
        }
    }
}

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
using HealthSync.IntegrationTests.Helpers;

namespace HealthSync.IntegrationTests.Controllers;

public class UserProfileControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UserProfileControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Auth header will be set per test as needed
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    [Fact]
    public async Task GetProfile_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/userprofile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithAuth_ShouldReturnUserProfile()
    {
        // Arrange
        var email = "test@example.com"; // Match the test token email
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Clear all existing data to prevent conflicts
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            dbContext.UserProfiles.RemoveRange(dbContext.UserProfiles);
            dbContext.UserRoles.RemoveRange(dbContext.UserRoles);
            dbContext.ApplicationUsers.RemoveRange(dbContext.ApplicationUsers);
            await dbContext.SaveChangesAsync();
        }

        // Seed user with profile
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            // Remove existing user if any
            var existingUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                var existingProfile = await dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == existingUser.UserId);
                if (existingProfile != null)
                {
                    dbContext.UserProfiles.Remove(existingProfile);
                }
                var existingUserRoles = dbContext.UserRoles.Where(ur => ur.UserId == existingUser.UserId);
                dbContext.UserRoles.RemoveRange(existingUserRoles);
                dbContext.ApplicationUsers.Remove(existingUser);
                await dbContext.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                UserId = 1, // Match the test token userId
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();

            // Add user profile
            dbContext.UserProfiles.Add(new UserProfile
            {
                UserId = user.UserId,
                FullName = "John Doe",
                Dob = new DateTime(1990, 5, 15),
                Gender = "Male",
                HeightCm = 180,
                WeightKg = 75,
                ActivityLevel = "Moderate"
            });

            await dbContext.SaveChangesAsync();
        }

        // Set auth header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateJwtToken());

        // Act
        var response = await _client.GetAsync("/api/userprofile");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("John Doe", profile.FullName);
        Assert.Equal(180, profile.HeightCm);
        Assert.Equal(75, profile.WeightKg);
    }

    [Fact]
    public async Task GetProfile_WithoutProfile_ShouldReturnNotFound()
    {
        // Arrange
        var email = "test2@example.com"; // Different email
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Clear all existing data to prevent conflicts
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            dbContext.UserProfiles.RemoveRange(dbContext.UserProfiles);
            dbContext.UserRoles.RemoveRange(dbContext.UserRoles);
            dbContext.ApplicationUsers.RemoveRange(dbContext.ApplicationUsers);
            await dbContext.SaveChangesAsync();
        }

        // Seed user without profile
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

        // Set auth header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateJwtToken());

        // Act
        var response = await _client.GetAsync("/api/userprofile");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithAuth_ShouldUpdateProfile()
    {
        // Arrange
        var email = "test@example.com"; // Match the test token email
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Clear all existing data to prevent conflicts
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            dbContext.UserProfiles.RemoveRange(dbContext.UserProfiles);
            dbContext.UserRoles.RemoveRange(dbContext.UserRoles);
            dbContext.ApplicationUsers.RemoveRange(dbContext.ApplicationUsers);
            await dbContext.SaveChangesAsync();
        }

        // Seed user with profile
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");

            // Remove existing user if any
            var existingUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                var existingProfile = await dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == existingUser.UserId);
                if (existingProfile != null)
                {
                    dbContext.UserProfiles.Remove(existingProfile);
                }
                var existingUserRoles = dbContext.UserRoles.Where(ur => ur.UserId == existingUser.UserId);
                dbContext.UserRoles.RemoveRange(existingUserRoles);
                dbContext.ApplicationUsers.Remove(existingUser);
                await dbContext.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                UserId = 1, // Match the test token userId
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole!.Id });
            await dbContext.SaveChangesAsync();

            dbContext.UserProfiles.Add(new UserProfile
            {
                UserId = user.UserId,
                FullName = "Old Name",
                Dob = new DateTime(1990, 1, 1),
                Gender = "Male",
                HeightCm = 175,
                WeightKg = 70
            });

            await dbContext.SaveChangesAsync();
        }

        // Set auth header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateJwtToken());

        // Act
        var updateRequest = new
        {
            FullName = "New Name",
            Dob = new DateTime(1990, 5, 15),
            Gender = "Male",
            HeightCm = 180,
            WeightKg = 75,
            ActivityLevel = "Active"
        };
        var response = await _client.PutAsJsonAsync("/api/userprofile", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Debug: Check PUT response content
        var putContent = await response.Content.ReadAsStringAsync();
        // Expected: "Profile updated successfully"

        // Verify update
        var getResponse = await _client.GetAsync("/api/userprofile");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var profile = await getResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("New Name", profile.FullName);
        Assert.Equal(180, profile.HeightCm);
        Assert.Equal(75, profile.WeightKg);
    }

    [Fact]
    public async Task UpdateProfile_WithoutProfile_ShouldReturnNotFound()
    {
        // Arrange
        var email = "test@example.com"; // Match the test token email
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Clean up any existing profile for userId=1 (from TestTokenGenerator)
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            
            var existingProfile = await dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == 1);
            if (existingProfile != null)
            {
                dbContext.UserProfiles.Remove(existingProfile);
            }
            
            var existingUserRoles = dbContext.UserRoles.Where(ur => ur.UserId == 1);
            dbContext.UserRoles.RemoveRange(existingUserRoles);
            
            var existingUser = await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.UserId == 1);
            if (existingUser != null)
            {
                dbContext.ApplicationUsers.Remove(existingUser);
            }
            
            await dbContext.SaveChangesAsync();
        }

        // Seed user without profile
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

        // Set auth header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TestTokenGenerator.GenerateJwtToken());

        // Act
        var updateRequest = new
        {
            FullName = "New Name",
            Dob = new DateTime(1990, 5, 15),
            Gender = "Male",
            HeightCm = 180,
            WeightKg = 75,
            ActivityLevel = "Active"
        };
        var response = await _client.PutAsJsonAsync("/api/userprofile", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var updateRequest = new
        {
            FullName = "Test Name",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175,
            WeightKg = 70
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/userprofile", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

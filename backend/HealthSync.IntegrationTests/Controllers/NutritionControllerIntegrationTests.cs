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

public class NutritionControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public NutritionControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task GetNutritionLogs_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/nutrition/nutrition-logs");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddFoodEntry_WithAuth_ShouldReturnOk()
    {
        // Arrange
        var email = $"nutrition_add_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);
        int foodItemId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            
            // Add Role
            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 }); // Customer

            // Add Food Item
            var food = new FoodItem 
            { 
                Name = "Nutrition Test Food", 
                ServingSize = 100, 
                ServingUnit = "g", 
                CaloriesKcal = 200, 
                ProteinG = 10, 
                CarbsG = 20, 
                FatG = 5 
            };
            dbContext.FoodItems.Add(food);
            
            await dbContext.SaveChangesAsync();
            foodItemId = food.FoodItemId;
        }

        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new AddFoodEntryDto
        {
            FoodItemId = foodItemId,
            Quantity = 1.5m, // 1.5 servings
            MealType = "Breakfast"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/nutrition/food-entry", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNutritionLogs_WithAuth_ShouldReturnLogs()
    {
        // Arrange
        var email = $"nutrition_get_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);
        int userId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();
            userId = user.UserId;

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 });

            // Seed a log
            var log = new NutritionLog
            {
                UserId = userId,
                LogDate = DateTime.UtcNow.Date,
                TotalCalories = 500,
                ProteinG = 30,
                CarbsG = 50,
                FatG = 10
            };
            dbContext.NutritionLogs.Add(log);
            await dbContext.SaveChangesAsync();
        }

        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/nutrition/nutrition-logs?startDate={DateTime.UtcNow.AddDays(-1):yyyy-MM-dd}&endDate={DateTime.UtcNow.AddDays(1):yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<NutritionLogDto>>();
        Assert.NotNull(logs);
        Assert.Contains(logs, l => l.TotalCalories == 500);
    }
}

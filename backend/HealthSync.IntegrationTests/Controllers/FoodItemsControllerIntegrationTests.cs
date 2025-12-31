using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests.Controllers;

public class FoodItemsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public FoodItemsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

    private string GenerateTestJwtToken()
    {
        // For manual token generation if needed, but we'll use the API login flow
        return ""; 
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
    public async Task GetFoodItems_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/fooditems");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFoodItems_WithCustomerAuth_ShouldReturnFoodItems()
    {
        // Arrange
        var email = $"food_cust_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            // Ensure data is seeded (Role 2 is Customer)
            var customerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == 2) ?? 
                               new Role { RoleName = "Customer", Description = "Test" };
                               
            if (customerRole.Id == 0) { dbContext.Roles.Add(customerRole); await dbContext.SaveChangesAsync(); }

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 });
            await dbContext.SaveChangesAsync();

            if (!await dbContext.FoodItems.AnyAsync())
            {
                dbContext.FoodItems.Add(new FoodItem { Name = "Test Apple", ServingSize = 100, ServingUnit = "g", CaloriesKcal = 50, ProteinG = 0, CarbsG = 14, FatG = 0 });
                await dbContext.SaveChangesAsync();
            }
        }

        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/fooditems");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var foods = await response.Content.ReadFromJsonAsync<List<FoodItemDto>>();
        Assert.NotNull(foods);
        Assert.NotEmpty(foods);
    }

    [Fact]
    public async Task CreateFoodItem_WithCustomerAuth_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"food_cust_forbidden_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

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

            // Role 2 (Customer) does NOT have FOOD_CREATE (302)
            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 });
            await dbContext.SaveChangesAsync();
        }

        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var command = new CreateFoodItemCommand
        {
            Name = "Forbidden Fruit",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 100,
            ProteinG = 0,
            CarbsG = 25,
            FatG = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/fooditems", command);

        // Assert
        // Expect Forbidden (403) because Customer lacks FOOD_CREATE permission
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateFoodItem_WithAdminAuth_ShouldReturnCreated()
    {
        // Arrange
        var email = $"food_admin_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

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

            // Role 1 (Admin) HAS FOOD_CREATE (302)
            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 1 });
            await dbContext.SaveChangesAsync();
        }

        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var command = new CreateFoodItemCommand
        {
            Name = "Admin Banana " + Guid.NewGuid().ToString().Substring(0, 5),
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 105,
            ProteinG = 1,
            CarbsG = 27,
            FatG = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/fooditems", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

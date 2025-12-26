using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests.E2E;

public class UserJourneyE2ETests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UserJourneyE2ETests(CustomWebApplicationFactory<Program> factory)
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
    public async Task CompleteUserJourney_LoginAndUseFeatures_ShouldWorkEndToEnd()
    {
        var email = $"journey_{Guid.NewGuid()}@example.com";
        var password = "SecurePass@123";
        var passwordHash = HashPassword(password); // Use SHA256 hash

        // Step 1: Seed user with Customer role and profile (role already seeded with permissions)
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            // Use pre-seeded Customer role that already has permissions
            var customerRole = await dbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            Assert.NotNull(customerRole); // Ensure role exists from seeding

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                Profile = new UserProfile
                {
                    FullName = "Journey User",
                    Dob = new DateTime(1992, 3, 15),
                    Gender = "Male",
                    HeightCm = 180,
                    WeightKg = 75,
                    ActivityLevel = "Active"
                }
            };
            dbContext.ApplicationUsers.Add(user);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
            await dbContext.SaveChangesAsync();
        }

        // Step 2: Login to get JWT token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        var token = authResponse!.Token;

        // Step 3: Get dashboard (authenticated request)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dashboardResponse = await _client.GetAsync("/api/dashboard/customer");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);

        // Step 4: Verify user and profile were created in database
        using (var scope2 = _factory.Services.CreateScope())
        {
            var dbContext2 = scope2.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            
            var userInDb = await dbContext2.ApplicationUsers
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Email == email);

            Assert.NotNull(userInDb);
            Assert.NotNull(userInDb.Profile);
            Assert.Equal(180, userInDb.Profile.HeightCm);
            Assert.Equal("Journey User", userInDb.Profile.FullName);
        }
    }

    [Fact]
    public async Task AdminJourney_ManageUsers_ShouldWorkEndToEnd()
    {
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin@123";

        // Seed admin user with Admin role
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var adminRole = await dbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            Assert.NotNull(adminRole);

            var admin = new ApplicationUser
            {
                Email = adminEmail,
                PasswordHash = HashPassword(adminPassword),
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(admin);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = admin.UserId, RoleId = adminRole.Id });
            await dbContext.SaveChangesAsync();
        }

        // Setup: Create admin user in database
        int adminUserId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            
            var adminRole = new Role { RoleName = "Admin", Description = "Administrator" };
            var adminUser = new ApplicationUser
            {
                Email = "admin@example.com",
                PasswordHash = HashPassword("Admin@123"), // Use SHA256 hash
                IsActive = true,
                UserRoles = new List<UserRole>
                {
                    new UserRole { Role = adminRole }
                }
            };

            dbContext.ApplicationUsers.Add(adminUser);
            await dbContext.SaveChangesAsync();
            adminUserId = adminUser.UserId;
        }

        // Step 1: Admin login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@example.com",
            Password = "Admin@123"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var adminToken = authResponse!.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Step 2: Get all users
        var getUsersResponse = await _client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.OK, getUsersResponse.StatusCode);

        // Step 3: Get admin statistics
        var statsResponse = await _client.GetAsync("/api/admin/statistics");
        Assert.Equal(HttpStatusCode.OK, statsResponse.StatusCode);

        var stats = await statsResponse.Content.ReadFromJsonAsync<AdminStatisticsDto>();
        Assert.NotNull(stats);
        Assert.NotNull(stats.UserStatistics);
        Assert.True(stats.UserStatistics.TotalUsers > 0);
    }

    [Fact]
    public async Task NutritionTracking_CompleteFlow_ShouldWork()
    {
        var email = $"nutrition_{Guid.NewGuid()}@example.com";
        var password = "Test@123";

        // Step 1: Create user and seed food items directly in database
        int oatmealId, bananaId;
        using (var userScope = _factory.Services.CreateScope())
        {
            var userDbContext = userScope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var customerRole = await userDbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            Assert.NotNull(customerRole);

            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = HashPassword(password),
                IsActive = true
            };
            userDbContext.ApplicationUsers.Add(user);
            await userDbContext.SaveChangesAsync();

            userDbContext.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRole.Id });
            
            // Seed food items for nutrition logging
            var oatmeal = new HealthSync.Domain.Entities.FoodItem
            {
                Name = "Oatmeal",
                ServingSize = 100m,
                ServingUnit = "g",
                CaloriesKcal = 150m,
                ProteinG = 5.0m,
                CarbsG = 27.0m,
                FatG = 3.0m
            };
            var banana = new HealthSync.Domain.Entities.FoodItem
            {
                Name = "Banana",
                ServingSize = 100m,
                ServingUnit = "g",
                CaloriesKcal = 105m,
                ProteinG = 1.3m,
                CarbsG = 27.0m,
                FatG = 0.4m
            };
            userDbContext.Set<HealthSync.Domain.Entities.FoodItem>().AddRange(oatmeal, banana);
            await userDbContext.SaveChangesAsync();
            
            oatmealId = oatmeal.FoodItemId;
            bananaId = banana.FoodItemId;
        }

        // Step 2: Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Step 2: Create nutrition log with food item references
        var today = DateTime.UtcNow.Date; // Use Date (not UtcNow) to match LogDate comparison
        var createLogResponse = await _client.PostAsJsonAsync("/api/nutrition/nutrition-logs", new
        {
            LogDate = today,
            FoodEntries = new[]
            {
                new
                {
                    FoodItemId = oatmealId,
                    Quantity = 1.0,
                    MealType = "Breakfast"
                },
                new
                {
                    FoodItemId = bananaId,
                    Quantity = 1.0,
                    MealType = "Breakfast"
                }
            }
        });

        Assert.Equal(HttpStatusCode.Created, createLogResponse.StatusCode);

        // Step 3: Get all nutrition logs
        var getLogResponse = await _client.GetAsync("/api/nutrition/nutrition-logs");
        Assert.Equal(HttpStatusCode.OK, getLogResponse.StatusCode);

        var nutritionLogs = await getLogResponse.Content.ReadFromJsonAsync<List<NutritionLogDto>>();
        Assert.NotNull(nutritionLogs);
        Assert.NotEmpty(nutritionLogs);
        var nutritionLog = nutritionLogs.FirstOrDefault(l => l.LogDate.Date == today);
        Assert.NotNull(nutritionLog);
        Assert.Equal(2, nutritionLog.FoodEntries.Count);
        // Verify food entries were created successfully
        // Note: TotalCalories calculation depends on CreateNutritionLogCommand logic
        Assert.True(nutritionLog.TotalCalories > 0, $"TotalCalories should be greater than 0, got {nutritionLog.TotalCalories}");

        // Step 4: Verify in database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
        
        var savedLog = await dbContext.NutritionLogs
            .Include(n => n.FoodEntries)
            .FirstOrDefaultAsync(n => n.User.Email == email);

        Assert.NotNull(savedLog);
        Assert.Equal(2, savedLog.FoodEntries.Count);
    }

    [Fact]
    public async Task ExerciseManagement_CreateUpdateDelete_ShouldWork()
    {
        // Setup admin with pre-seeded Admin role (has all permissions)
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthSyncDbContext>();
            var adminRole = await dbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            Assert.NotNull(adminRole); // Ensure Admin role exists from seeding

            var admin = new ApplicationUser
            {
                Email = "exercise_admin@example.com",
                PasswordHash = HashPassword("Admin@123"), // Use SHA256 hash
                IsActive = true
            };
            dbContext.ApplicationUsers.Add(admin);
            await dbContext.SaveChangesAsync();

            dbContext.UserRoles.Add(new UserRole { UserId = admin.UserId, RoleId = adminRole.Id });
            await dbContext.SaveChangesAsync();
        }

        // Login as admin
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "exercise_admin@example.com",
            Password = "Admin@123"
        });

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Step 1: Create exercise
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", new
        {
            ExerciseName = "Bench Press",
            Description = "Chest exercise",
            MuscleGroup = "Chest",
            EquipmentNeeded = "Barbell",
            DifficultyLevel = "Intermediate"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        var exerciseId = createdExercise!.ExerciseId;

        // Step 2: Get exercise
        var getResponse = await _client.GetAsync($"/api/exercises/{exerciseId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // Step 3: Update exercise
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{exerciseId}", new
        {
            ExerciseId = exerciseId,
            ExerciseName = "Bench Press",
            Description = "Updated description",
            MuscleGroup = "Chest",
            EquipmentNeeded = "Barbell",
            DifficultyLevel = "Advanced"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Step 4: Delete exercise
        var deleteResponse = await _client.DeleteAsync($"/api/exercises/{exerciseId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Step 5: Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/exercises/{exerciseId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldBeBlocked()
    {
        // Try to access protected endpoint without token
        var response = await _client.GetAsync("/api/dashboard/customer");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // Try with invalid token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_token");
        response = await _client.GetAsync("/api/dashboard/customer");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

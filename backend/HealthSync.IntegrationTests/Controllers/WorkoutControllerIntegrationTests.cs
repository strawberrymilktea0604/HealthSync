using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using HealthSync.Application.DTOs;
using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using HealthSync.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthSync.IntegrationTests.Controllers;

public class WorkoutControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public WorkoutControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

    // Hàm tạo Token chuẩn
    private string GenerateTestJwtToken()
    {
        // Lấy key từ Env (đã load thành công)
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrEmpty(secretKey))
        {
            secretKey = "ThisIsATestSecretKeyForIntegrationTestingPurposesOnly123456";
        }
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                // Thêm các claim khác nếu API yêu cầu
            }),
            Expires = DateTime.UtcNow.AddMinutes(10),
            Issuer = "HealthSync",   // Phải khớp với appsettings
            Audience = "HealthSyncUsers", // Phải khớp với appsettings
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token); // Trả về chuỗi xxx.yyy.zzz
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
    public async Task GetExercises_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/workout/exercises");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetExercises_WithAuth_ShouldReturnExercises()
    {
        // Arrange
        var email = $"workout_{Guid.NewGuid()}@example.com";
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
            
            // Add sample exercises
            dbContext.Exercises.AddRange(
                new Exercise
                {
                    Name = "Push-ups",
                    Description = "Upper body exercise",
                    MuscleGroup = "Chest",
                    Difficulty = "Beginner"
                },
                new Exercise
                {
                    Name = "Squats",
                    Description = "Lower body exercise",
                    MuscleGroup = "Legs",
                    Difficulty = "Beginner"
                }
            );
            
            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/workout/exercises");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var exercises = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>();
        Assert.NotNull(exercises);
        Assert.True(exercises.Count >= 2);
    }

    [Fact]
    public async Task GetExercises_WithFilters_ShouldReturnFilteredExercises()
    {
        // Arrange
        var email = $"workoutfilter_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        // Seed user and exercises
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
        var response = await _client.GetAsync("/api/workout/exercises?muscleGroup=Chest&difficulty=Beginner");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new 
        { 
            WorkoutDate = DateTime.UtcNow,
            ExerciseSessions = new[]
            {
                new { ExerciseId = 1, Sets = 3, Reps = 10, WeightKg = 20.0 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workout/workout-logs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkoutLog_WithAuth_ShouldCreateLog()
    {
        // Arrange
        var email = $"workoutlog_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int exerciseId;

        // Seed user and exercise
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

            var exercise = new Exercise
            {
                Name = "Bench Press",
                Description = "Chest exercise",
                MuscleGroup = "Chest",
                Difficulty = "Intermediate"
            };
            dbContext.Exercises.Add(exercise);
            await dbContext.SaveChangesAsync();

            exerciseId = exercise.ExerciseId;
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new 
        { 
            WorkoutDate = DateTime.UtcNow,
            TotalDurationMinutes = 45,
            CaloriesBurned = 300,
            Notes = "Great workout!",
            ExerciseSessions = new[]
            {
                new 
                { 
                    ExerciseId = exerciseId, 
                    Sets = 3, 
                    Reps = 10, 
                    WeightKg = 60.0,
                    DurationMinutes = 15
                }
            }
        };
        var response = await _client.PostAsJsonAsync("/api/workout/workout-logs", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/workout/workout-logs");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithAuth_ShouldReturnUserLogs()
    {
        // Arrange
        var email = $"workoutlogs_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int userId;

        // Seed user and workout logs
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

            // Add workout logs
            dbContext.WorkoutLogs.AddRange(
                new WorkoutLog
                {
                    UserId = userId,
                    WorkoutDate = DateTime.UtcNow.AddDays(-1),
                    DurationMin = 30,
                    Notes = "Morning workout"
                },
                new WorkoutLog
                {
                    UserId = userId,
                    WorkoutDate = DateTime.UtcNow,
                    DurationMin = 45,
                    Notes = "Evening workout"
                }
            );

            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/workout/workout-logs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<WorkoutLogDto>>();
        Assert.NotNull(logs);
        Assert.True(logs.Count >= 2);
    }

    [Fact]
    public async Task GetWorkoutLogs_WithDateFilter_ShouldReturnFilteredLogs()
    {
        // Arrange
        var email = $"workoutdate_{Guid.NewGuid()}@example.com";
        var password = "Test@123456";
        var passwordHash = HashPassword(password);

        int userId;

        // Seed user and workout logs
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

            // Add workout logs with different dates
            dbContext.WorkoutLogs.AddRange(
                new WorkoutLog
                {
                    UserId = userId,
                    WorkoutDate = DateTime.UtcNow.AddDays(-10),
                    DurationMin = 30
                },
                new WorkoutLog
                {
                    UserId = userId,
                    WorkoutDate = DateTime.UtcNow,
                    DurationMin = 45
                }
            );

            await dbContext.SaveChangesAsync();
        }

        // Login
        var token = await LoginAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/workout/workout-logs?startDate={startDate}&endDate={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<WorkoutLogDto>>();
        Assert.NotNull(logs);
        // Should only return logs within the date range
        Assert.True(logs.Count >= 1);
    }
}

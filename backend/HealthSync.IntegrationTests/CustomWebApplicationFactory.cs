using HealthSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace HealthSync.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set JWT secret as environment variable (highest priority)
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "ThisIsATestSecretKeyForIntegrationTestingPurposesOnly123456");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration sources (including appsettings.json)
            config.Sources.Clear();
            
            // Add test configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpirationMinutes"] = "60",
                ["ConnectionStrings:DefaultConnection"] = "InMemoryDatabase",
                ["MinIO:Endpoint"] = "localhost:9000",
                ["MinIO:AccessKey"] = "test",
                ["MinIO:SecretKey"] = "test123456",
                ["MinIO:BucketName"] = "test-bucket",
                ["MinIO:UseSSL"] = "false",
                ["Email:SmtpServer"] = "smtp.test.com",
                ["Email:SmtpPort"] = "587",
                ["Email:SenderEmail"] = "test@test.com",
                ["Email:SenderPassword"] = "testpassword",
                ["Gemini:ApiKey"] = "test-api-key"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<HealthSyncDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<HealthSyncDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<HealthSyncDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db);
            }
        });

        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(HealthSyncDbContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed Roles
        var customerRole = new HealthSync.Domain.Entities.Role
        {
            RoleName = "Customer",
            Description = "Regular customer role"
        };
        var adminRole = new HealthSync.Domain.Entities.Role
        {
            RoleName = "Admin",
            Description = "Administrator role"
        };
        context.Roles.AddRange(customerRole, adminRole);
        context.SaveChanges();

        // Seed Permissions (all permission codes)
        var permissions = new List<HealthSync.Domain.Entities.Permission>
        {
            new() { PermissionCode = "USER_READ", Description = "Read Users" },
            new() { PermissionCode = "USER_BAN", Description = "Ban Users" },
            new() { PermissionCode = "USER_UPDATE_ROLE", Description = "Update User Role" },
            new() { PermissionCode = "USER_DELETE", Description = "Delete User" },
            new() { PermissionCode = "EXERCISE_READ", Description = "Read Exercises" },
            new() { PermissionCode = "EXERCISE_CREATE", Description = "Create Exercise" },
            new() { PermissionCode = "EXERCISE_UPDATE", Description = "Update Exercise" },
            new() { PermissionCode = "EXERCISE_DELETE", Description = "Delete Exercise" },
            new() { PermissionCode = "FOOD_READ", Description = "Read Foods" },
            new() { PermissionCode = "FOOD_CREATE", Description = "Create Food" },
            new() { PermissionCode = "FOOD_UPDATE", Description = "Update Food" },
            new() { PermissionCode = "FOOD_DELETE", Description = "Delete Food" },
            new() { PermissionCode = "WORKOUT_LOG_READ", Description = "Read Workout Logs" },
            new() { PermissionCode = "WORKOUT_LOG_CREATE", Description = "Create Workout Log" },
            new() { PermissionCode = "WORKOUT_LOG_UPDATE", Description = "Update Workout Log" },
            new() { PermissionCode = "WORKOUT_LOG_DELETE", Description = "Delete Workout Log" },
            new() { PermissionCode = "NUTRITION_LOG_READ", Description = "Read Nutrition Logs" },
            new() { PermissionCode = "NUTRITION_LOG_CREATE", Description = "Create Nutrition Log" },
            new() { PermissionCode = "NUTRITION_LOG_UPDATE", Description = "Update Nutrition Log" },
            new() { PermissionCode = "NUTRITION_LOG_DELETE", Description = "Delete Nutrition Log" },
            new() { PermissionCode = "GOAL_READ", Description = "Read Goals" },
            new() { PermissionCode = "GOAL_CREATE", Description = "Create Goal" },
            new() { PermissionCode = "GOAL_UPDATE", Description = "Update Goal" },
            new() { PermissionCode = "GOAL_DELETE", Description = "Delete Goal" },
            new() { PermissionCode = "DASHBOARD_VIEW", Description = "View Dashboard" },
            new() { PermissionCode = "DASHBOARD_ADMIN", Description = "Admin Dashboard" }
        };
        context.Permissions.AddRange(permissions);
        context.SaveChanges();

        // Assign permissions to Customer role (basic permissions for regular users)
        var customerPermissions = context.Permissions.Where(p =>
            p.PermissionCode == "EXERCISE_READ" ||
            p.PermissionCode == "EXERCISE_CREATE" ||
            p.PermissionCode == "EXERCISE_UPDATE" ||
            p.PermissionCode == "EXERCISE_DELETE" ||
            p.PermissionCode == "FOOD_READ" ||
            p.PermissionCode == "WORKOUT_LOG_READ" ||
            p.PermissionCode == "WORKOUT_LOG_CREATE" ||
            p.PermissionCode == "WORKOUT_LOG_UPDATE" ||
            p.PermissionCode == "WORKOUT_LOG_DELETE" ||
            p.PermissionCode == "NUTRITION_LOG_READ" ||
            p.PermissionCode == "NUTRITION_LOG_CREATE" ||
            p.PermissionCode == "NUTRITION_LOG_UPDATE" ||
            p.PermissionCode == "NUTRITION_LOG_DELETE" ||
            p.PermissionCode == "GOAL_READ" ||
            p.PermissionCode == "GOAL_CREATE" ||
            p.PermissionCode == "GOAL_UPDATE" ||
            p.PermissionCode == "GOAL_DELETE" ||
            p.PermissionCode == "DASHBOARD_VIEW"
        ).ToList();

        foreach (var permission in customerPermissions)
        {
            context.RolePermissions.Add(new HealthSync.Domain.Entities.RolePermission
            {
                RoleId = customerRole.Id,
                PermissionId = permission.Id
            });
        }

        // Assign all permissions to Admin role
        var allPermissions = context.Permissions.ToList();
        foreach (var permission in allPermissions)
        {
            context.RolePermissions.Add(new HealthSync.Domain.Entities.RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }

        context.SaveChanges();
    }
}

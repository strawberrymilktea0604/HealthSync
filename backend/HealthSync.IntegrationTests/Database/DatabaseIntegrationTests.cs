using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthSync.IntegrationTests.Database;

public class DatabaseIntegrationTests
{
    private HealthSyncDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HealthSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HealthSyncDbContext(options);
    }

    [Fact]
    public async Task ApplicationUser_WithProfile_ShouldMaintainRelationship()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new ApplicationUser
        {
            Email = "test@example.com",
            PasswordHash = "hashed",
            IsActive = true,
            Profile = new UserProfile
            {
                FullName = "Test User",
                Dob = new DateTime(1990, 1, 1),
                Gender = "Male",
                HeightCm = 175.0m,
                WeightKg = 70.0m
            }
        };

        // Act
        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.ApplicationUsers
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == "test@example.com");

        Assert.NotNull(savedUser);
        Assert.NotNull(savedUser.Profile);
        Assert.Equal("Test User", savedUser.Profile.FullName);
        Assert.Equal(user.UserId, savedUser.Profile.UserId);
    }

    [Fact]
    public async Task DeleteUser_WithCascade_ShouldDeleteProfile()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new ApplicationUser
        {
            Email = "cascade@example.com",
            PasswordHash = "hashed",
            Profile = new UserProfile
            {
                FullName = "Cascade Test",
                Dob = DateTime.Now.AddYears(-25),
                Gender = "Male"
            }
        };

        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();

        var userId = user.UserId;

        // Act
        context.ApplicationUsers.Remove(user);
        await context.SaveChangesAsync();

        // Assert
        var deletedUser = await context.ApplicationUsers.FindAsync(userId);
        var orphanedProfile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        Assert.Null(deletedUser);
        Assert.Null(orphanedProfile); // Should be cascade deleted
    }

    [Fact]
    public async Task UserRoles_ManyToMany_ShouldWorkCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var adminRole = new Role
        {
            RoleName = "Admin",
            Description = "Administrator"
        };

        var customerRole = new Role
        {
            RoleName = "Customer",
            Description = "Customer"
        };

        var user = new ApplicationUser
        {
            Email = "roles@example.com",
            PasswordHash = "hashed",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = adminRole },
                new UserRole { Role = customerRole }
            }
        };

        // Act
        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.ApplicationUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == "roles@example.com");

        Assert.NotNull(savedUser);
        Assert.Equal(2, savedUser.UserRoles.Count);
        Assert.Contains(savedUser.UserRoles, ur => ur.Role.RoleName == "Admin");
        Assert.Contains(savedUser.UserRoles, ur => ur.Role.RoleName == "Customer");
    }

    [Fact]
    public async Task WorkoutLog_WithUser_ShouldSaveCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new ApplicationUser
        {
            Email = "workout@example.com",
            PasswordHash = "hashed"
        };

        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();

        var workout = new WorkoutLog
        {
            UserId = user.UserId,
            WorkoutDate = DateTime.UtcNow,
            DurationMin = 60,
            Notes = "Morning workout"
        };

        // Act
        context.WorkoutLogs.Add(workout);
        await context.SaveChangesAsync();

        // Assert
        var savedWorkout = await context.WorkoutLogs
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.WorkoutLogId == workout.WorkoutLogId);

        Assert.NotNull(savedWorkout);
        Assert.Equal(user.UserId, savedWorkout.UserId);
        Assert.Equal("workout@example.com", savedWorkout.User.Email);
    }

    [Fact]
    public async Task NutritionLog_WithFoodEntries_ShouldMaintainCollectionRelationship()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new ApplicationUser
        {
            Email = "nutrition@example.com",
            PasswordHash = "hashed"
        };

        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();

        // Create food items first
        var apple = new FoodItem
        {
            Name = "Apple",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 95,
            ProteinG = 0.5m,
            CarbsG = 25m,
            FatG = 0.3m
        };
        var chicken = new FoodItem
        {
            Name = "Chicken Breast",
            ServingSize = 100,
            ServingUnit = "g",
            CaloriesKcal = 165,
            ProteinG = 31m,
            CarbsG = 0m,
            FatG = 3.6m
        };
        context.FoodItems.Add(apple);
        context.FoodItems.Add(chicken);
        await context.SaveChangesAsync();

        var nutritionLog = new NutritionLog
        {
            UserId = user.UserId,
            LogDate = DateTime.UtcNow,
            TotalCalories = 260,
            ProteinG = 31.5m,
            CarbsG = 25m,
            FatG = 3.9m,
            FoodEntries = new List<FoodEntry>
            {
                new FoodEntry
                {
                    FoodItemId = apple.FoodItemId,
                    Quantity = 1,
                    MealType = "Breakfast",
                    CaloriesKcal = 95,
                    ProteinG = 0.5m,
                    CarbsG = 25m,
                    FatG = 0.3m
                },
                new FoodEntry
                {
                    FoodItemId = chicken.FoodItemId,
                    Quantity = 1,
                    MealType = "Lunch",
                    CaloriesKcal = 165,
                    ProteinG = 31m,
                    CarbsG = 0m,
                    FatG = 3.6m
                }
            }
        };

        // Act
        context.NutritionLogs.Add(nutritionLog);
        await context.SaveChangesAsync();

        // Assert
        var savedLog = await context.NutritionLogs
            .Include(n => n.FoodEntries)
            .FirstOrDefaultAsync(n => n.NutritionLogId == nutritionLog.NutritionLogId);

        Assert.NotNull(savedLog);
        Assert.Equal(2, savedLog.FoodEntries.Count);
        var foodEntriesWithItems = await context.FoodEntries
            .Include(f => f.FoodItem)
            .Where(f => f.NutritionLogId == savedLog.NutritionLogId)
            .ToListAsync();
        Assert.Contains(foodEntriesWithItems, f => f.FoodItem.Name == "Apple");
        Assert.Contains(foodEntriesWithItems, f => f.FoodItem.Name == "Chicken Breast");
    }

    [Fact]
    public async Task ConcurrentUpdates_ShouldHandleCorrectly()
    {
        // Arrange
        using var context1 = CreateInMemoryContext();
        using var context2 = CreateInMemoryContext();

        // Share same in-memory database
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<HealthSyncDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        using var setupContext = new HealthSyncDbContext(options);
        var user = new ApplicationUser
        {
            Email = "concurrent@example.com",
            PasswordHash = "hashed",
            Profile = new UserProfile
            {
                FullName = "Original Name",
                Dob = DateTime.Now.AddYears(-30),
                Gender = "Male"
            }
        };
        setupContext.ApplicationUsers.Add(user);
        await setupContext.SaveChangesAsync();
        var userId = user.UserId;

        // Act - Concurrent updates
        using var updateContext1 = new HealthSyncDbContext(options);
        using var updateContext2 = new HealthSyncDbContext(options);

        var user1 = await updateContext1.ApplicationUsers
            .Include(u => u.Profile)
            .FirstAsync(u => u.UserId == userId);
        
        var user2 = await updateContext2.ApplicationUsers
            .Include(u => u.Profile)
            .FirstAsync(u => u.UserId == userId);

        user1.Profile!.FullName = "Updated by Context 1";
        user2.Profile!.FullName = "Updated by Context 2";

        await updateContext1.SaveChangesAsync();
        await updateContext2.SaveChangesAsync(); // Last write wins

        // Assert
        using var verifyContext = new HealthSyncDbContext(options);
        var finalUser = await verifyContext.ApplicationUsers
            .Include(u => u.Profile)
            .FirstAsync(u => u.UserId == userId);

        Assert.Equal("Updated by Context 2", finalUser.Profile!.FullName);
    }

    [Fact]
    public async Task RolePermissions_ShouldLoadCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var permission1 = new Permission
        {
            PermissionCode = "USER_CREATE",
            Description = "Create users"
        };

        var permission2 = new Permission
        {
            PermissionCode = "USER_DELETE",
            Description = "Delete users"
        };

        var adminRole = new Role
        {
            RoleName = "Admin",
            RolePermissions = new List<RolePermission>
            {
                new RolePermission { Permission = permission1 },
                new RolePermission { Permission = permission2 }
            }
        };

        // Act
        context.Roles.Add(adminRole);
        await context.SaveChangesAsync();

        // Assert
        var savedRole = await context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleName == "Admin");

        Assert.NotNull(savedRole);
        Assert.Equal(2, savedRole.RolePermissions.Count);
        Assert.Contains(savedRole.RolePermissions, rp => rp.Permission.PermissionCode == "USER_CREATE");
        Assert.Contains(savedRole.RolePermissions, rp => rp.Permission.PermissionCode == "USER_DELETE");
    }
}

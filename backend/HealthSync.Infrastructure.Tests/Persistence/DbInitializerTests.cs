using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Persistence;

public class DbInitializerTests
{
    private DbContextOptions<HealthSyncDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<HealthSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
    }

    [Fact]
    public void SeedData_WithEmptyDatabase_SeedsAllData()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        
        // Ensure Role exists (DbInitializer depends on Customer Role)
        context.Roles.Add(new Role { Id = 2, RoleName = "Customer" });
        context.SaveChanges();

        // Act
        DbInitializer.SeedData(context);

        // Assert
        Assert.True(context.Exercises.Count() > 0, "Exercises should be seeded");
        Assert.True(context.FoodItems.Count() > 0, "FoodItems should be seeded");
        Assert.True(context.ApplicationUsers.Count() >= 50, "Users should be seeded to at least 50");
        
        // Verify relationships
        var userWithProfile = context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.WorkoutLogs)
            .Include(u => u.NutritionLogs)
            .Include(u => u.Goals)
            .First(u => u.Profile != null); // Get a seeded user

        Assert.NotNull(userWithProfile.Profile);
        Assert.NotEmpty(userWithProfile.WorkoutLogs);
        Assert.NotEmpty(userWithProfile.NutritionLogs);
        Assert.NotEmpty(userWithProfile.Goals);
    }

    [Fact]
    public void SeedData_WithExistingData_DoesNotDuplicateMasterData()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        
        context.Roles.Add(new Role { Id = 2, RoleName = "Customer" });
        context.SaveChanges();

        // First Seed
        DbInitializer.SeedData(context);
        var initialExerciseCount = context.Exercises.Count();
        var initialFoodCount = context.FoodItems.Count();

        // Act - Second Seed
        DbInitializer.SeedData(context);

        // Assert
        Assert.Equal(initialExerciseCount, context.Exercises.Count());
        Assert.Equal(initialFoodCount, context.FoodItems.Count());
    }

    [Fact]
    public void SeedExercises_WhenThresholdReached_ReturnsEarly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        
        // Add 35 dummy exercises > 30 threshold
        for (int i = 0; i < 35; i++)
        {
            context.Exercises.Add(new Exercise 
            { 
                Name = $"Dummy {i}", 
                MuscleGroup = "Chest", 
                Difficulty = "Beginner", 
                Equipment = "None" 
            });
        }
        context.SaveChanges();
        var countBefore = context.Exercises.Count();

        // Act
        // We can't call private method SeedExercises directly, but SeedData calls it.
        // We need to make sure SeedData doesn't crash or add more.
        DbInitializer.SeedData(context);

        // Assert
        Assert.Equal(countBefore, context.Exercises.Count());
        // Verify standard exercises were NOT added (e.g. "Push-ups" from SeedExercises)
        Assert.DoesNotContain(context.Exercises, e => e.Name == "Incline Bench Press");
    }
    
    [Fact]
    public void SeedFoodItems_WhenThresholdReached_ReturnsEarly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        
        // Add 55 dummy food items > 50 threshold
        for (int i = 0; i < 55; i++)
        {
            context.FoodItems.Add(new FoodItem 
            { 
                Name = $"Dummy {i}", 
                ServingSize = 100, 
                ServingUnit = "g" 
            });
        }
        context.SaveChanges();
        var countBefore = context.FoodItems.Count();

        // Act
        DbInitializer.SeedData(context);

        // Assert
        Assert.Equal(countBefore, context.FoodItems.Count());
        Assert.DoesNotContain(context.FoodItems, f => f.Name == "Beef Steak");
    }

    [Fact]
    public void SeedUsers_WhenThresholdReached_ReturnsEarly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        
        context.Roles.Add(new Role { Id = 2, RoleName = "Customer" });
        
        // Add 55 users > 50 threshold
        var users = new List<ApplicationUser>();
        for (int i = 0; i < 55; i++)
        {
            users.Add(new ApplicationUser 
            { 
                Email = $"user{i}@test.com", 
                PasswordHash = "hash",
                IsActive = true
            });
        }
        context.ApplicationUsers.AddRange(users);
        context.SaveChanges();
        
        var countBefore = context.ApplicationUsers.Count();

        // Act
        DbInitializer.SeedData(context);

        // Assert
        Assert.Equal(countBefore, context.ApplicationUsers.Count());
    }
}

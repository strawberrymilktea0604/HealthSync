using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Persistence;

public class HealthSyncDbContextTests
{
    private DbContextOptions<HealthSyncDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<HealthSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private HealthSyncDbContext CreateAndSeedContext()
    {
        var options = CreateNewContextOptions();
        var context = new HealthSyncDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void IApplicationDbContext_Add_AddsEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        var iContext = context as HealthSync.Domain.Interfaces.IApplicationDbContext;
        
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        // Act
        iContext.Add(user);
        context.SaveChanges();

        // Assert
        Assert.Equal(1, context.ApplicationUsers.Count());
    }

    [Fact]
    public void IApplicationDbContext_Update_UpdatesEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        var iContext = context as HealthSync.Domain.Interfaces.IApplicationDbContext;
        
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            IsActive = true
        };
        
        context.ApplicationUsers.Add(user);
        context.SaveChanges();

        // Act
        user.IsActive = false;
        iContext.Update(user);
        context.SaveChanges();

        // Assert
        var updatedUser = context.ApplicationUsers.First();
        Assert.False(updatedUser.IsActive);
    }

    [Fact]
    public void IApplicationDbContext_Remove_RemovesEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        var iContext = context as HealthSync.Domain.Interfaces.IApplicationDbContext;
        
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        
        context.ApplicationUsers.Add(user);
        context.SaveChanges();

        // Act
        iContext.Remove(user);
        context.SaveChanges();

        // Assert
        Assert.Equal(0, context.ApplicationUsers.Count());
    }

    [Fact]
    public void OnModelCreating_ConfiguresEntitiesCorrectly()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act - The context is created, OnModelCreating is called automatically

        // Assert - Verify seed data is present
        Assert.NotEmpty(context.FoodItems);
        Assert.NotEmpty(context.Exercises);
        Assert.NotEmpty(context.Roles);
        Assert.NotEmpty(context.Permissions);
    }

    [Fact]
    public void OnModelCreating_SeedsFoodItems()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act
        var foodItems = context.FoodItems.ToList();

        // Assert
        Assert.Equal(10, foodItems.Count);
        Assert.Contains(foodItems, f => f.Name == "Chicken Breast");
        Assert.Contains(foodItems, f => f.Name == "Brown Rice");
        Assert.Contains(foodItems, f => f.Name == "Banana");
    }

    [Fact]
    public void OnModelCreating_SeedsExercises()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act
        var exercises = context.Exercises.ToList();

        // Assert
        Assert.Equal(15, exercises.Count);
        Assert.Contains(exercises, e => e.Name == "Push-ups");
        Assert.Contains(exercises, e => e.Name == "Bench Press");
        Assert.Contains(exercises, e => e.Name == "Pull-ups");
        Assert.Contains(exercises, e => e.Name == "Squats");
    }

    [Fact]
    public void OnModelCreating_SeedsRoles()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act
        var roles = context.Roles.ToList();

        // Assert
        Assert.Equal(2, roles.Count);
        Assert.Contains(roles, r => r.RoleName == "Admin");
        Assert.Contains(roles, r => r.RoleName == "Customer");
    }

    [Fact]
    public void OnModelCreating_SeedsPermissions()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act
        var permissions = context.Permissions.ToList();

        // Assert
        Assert.True(permissions.Count > 0);
        Assert.Contains(permissions, p => p.PermissionCode == "USER_READ");
        Assert.Contains(permissions, p => p.PermissionCode == "EXERCISE_CREATE");
        Assert.Contains(permissions, p => p.PermissionCode == "FOOD_DELETE");
    }

    [Fact]
    public void OnModelCreating_SeedsRolePermissions()
    {
        // Arrange
        using var context = CreateAndSeedContext();

        // Act
        var rolePermissions = context.RolePermissions.ToList();

        // Assert
        Assert.True(rolePermissions.Count > 0);
        
        // Verify Admin has comprehensive permissions
        var adminRoleId = context.Roles.First(r => r.RoleName == "Admin").Id;
        var adminPermissions = rolePermissions.Where(rp => rp.RoleId == adminRoleId).ToList();
        Assert.True(adminPermissions.Count > 10);
        
        // Verify Customer has limited permissions
        var customerRoleId = context.Roles.First(r => r.RoleName == "Customer").Id;
        var customerPermissions = rolePermissions.Where(rp => rp.RoleId == customerRoleId).ToList();
        Assert.True(customerPermissions.Count > 5);
    }

    [Fact]
    public void IApplicationDbContext_QueryableProperties_ReturnQueryable()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);
        var iContext = context as HealthSync.Domain.Interfaces.IApplicationDbContext;

        // Act &amp; Assert - Verify all queryable properties are accessible
        Assert.NotNull(iContext.ApplicationUsers);
        Assert.NotNull(iContext.UserProfiles);
        Assert.NotNull(iContext.Goals);
        Assert.NotNull(iContext.ProgressRecords);
        Assert.NotNull(iContext.WorkoutLogs);
        Assert.NotNull(iContext.ExerciseSessions);
        Assert.NotNull(iContext.Exercises);
        Assert.NotNull(iContext.NutritionLogs);
        Assert.NotNull(iContext.FoodEntries);
        Assert.NotNull(iContext.FoodItems);
        Assert.NotNull(iContext.Roles);
        Assert.NotNull(iContext.Permissions);
        Assert.NotNull(iContext.RolePermissions);
        Assert.NotNull(iContext.UserRoles);
        Assert.NotNull(iContext.ChatMessages);
    }

    [Fact]
    public void DbContext_ConfiguresRelationships_Correctly()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new HealthSyncDbContext(options);

        // Act - Create related entities
        var user = new ApplicationUser
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var profile = new UserProfile
        {
            User = user,
            FullName = "Test User",
            Dob = DateTime.UtcNow.AddYears(-25)
        };

        context.ApplicationUsers.Add(user);
        context.UserProfiles.Add(profile);
        context.SaveChanges();

        // Assert
        var savedUser = context.ApplicationUsers.Include(u => u.Profile).First();
        Assert.NotNull(savedUser.Profile);
        Assert.Equal("Test User", savedUser.Profile.FullName);
    }
}



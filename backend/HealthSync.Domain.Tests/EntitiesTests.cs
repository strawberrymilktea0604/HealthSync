using HealthSync.Domain.Entities;
using Xunit;

namespace HealthSync.Domain.Tests;

public class EntitiesTests
{
    [Fact]
    public void ApplicationUser_ShouldHaveDefaultValues()
    {
        var user = new ApplicationUser();
        Assert.Equal(0, user.UserId);
        Assert.Equal(string.Empty, user.Email);
        Assert.True(user.IsActive);
        Assert.NotEqual(default(DateTime), user.CreatedAt);
    }

    [Fact]
    public void Goal_ShouldInitializeCorrectly()
    {
        var goal = new Goal
        {
            Type = "weight_loss",
            TargetValue = 10.5m,
            StartDate = DateTime.UtcNow
        };
        Assert.Equal("weight_loss", goal.Type);
        Assert.Equal(10.5m, goal.TargetValue);
        Assert.Equal("active", goal.Status);
    }

    [Fact]
    public void Exercise_ShouldHaveProperties()
    {
        var exercise = new Exercise();
        Assert.NotNull(exercise);
        // Add more assertions based on actual properties
    }

    [Fact]
    public void FoodItem_ShouldHaveProperties()
    {
        var food = new FoodItem();
        Assert.NotNull(food);
        // Add more assertions based on actual properties
    }

    [Fact]
    public void UserProfile_ShouldHaveProperties()
    {
        var profile = new UserProfile();
        Assert.NotNull(profile);
        // Add more assertions based on actual properties
    }

    [Fact]
    public void WorkoutLog_ShouldHaveProperties()
    {
        var log = new WorkoutLog();
        Assert.NotNull(log);
        // Add more assertions based on actual properties
    }
}
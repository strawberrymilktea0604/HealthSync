using HealthSync.Domain.Entities;
using Xunit;

namespace HealthSync.Domain.Tests.Entities;

public class UserProfileTests
{
    [Fact]
    public void IsComplete_ShouldReturnTrue_WhenAllFieldsValid()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1), // 34 years old
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m,
            ActivityLevel = "Moderate"
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenFullNameEmpty()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "", // Empty
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenFullNameWhitespace()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "   ", // Whitespace only
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenGenderEmpty()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "", // Empty
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenGenderUnknown()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Unknown", // Unknown
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenHeightCmZero()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 0, // Zero
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenHeightCmNegative()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = -10, // Negative
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenWeightKgZero()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 0 // Zero
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenWeightKgNegative()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = -5 // Negative
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenDobTooYoung()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = DateTime.UtcNow.AddYears(-5), // 5 years old - too young
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnFalse_WhenDobInFuture()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = DateTime.UtcNow.AddYears(1), // Future date
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsComplete_ShouldReturnTrue_WhenDobExactly10YearsAgo()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = DateTime.UtcNow.AddYears(-10).AddDays(-1), // Just over 10 years old (more than 10 years ago)
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsComplete_ShouldHandleDefaultValues()
    {
        // Arrange - Using default constructor values
        var profile = new UserProfile
        {
            UserId = 1,
            FullName = "John Doe",
            Dob = new DateTime(1990, 1, 1),
            Gender = "Male",
            HeightCm = 175.5m,
            WeightKg = 70.0m
            // ActivityLevel defaults to "Moderate"
        };

        // Act
        var result = profile.IsComplete();

        // Assert
        Assert.True(result);
    }
}
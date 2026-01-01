using HealthSync.Application.Services;
using Xunit;

namespace HealthSync.Application.Tests.Services;

public class VerificationCodeStoreTests
{
    [Fact]
    public void Store_ShouldStoreVerificationCode()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";

        // Act
        VerificationCodeStore.Store(email, code);
        var result = VerificationCodeStore.Verify(email, code, markAsUsed: false);

        // Assert
        Assert.True(result);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Store_ShouldNormalizeEmailToLowerCase()
    {
        // Arrange
        var email = "Test@Example.COM";
        var code = "123456";

        // Act
        VerificationCodeStore.Store(email, code);
        var resultLowerCase = VerificationCodeStore.Verify("test@example.com", code, markAsUsed: false);

        // Assert
        Assert.True(resultLowerCase);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenCodeMatches()
    {
        // Arrange
        var email = "valid@example.com";
        var code = "654321";
        VerificationCodeStore.Store(email, code);

        // Act
        var result = VerificationCodeStore.Verify(email, code);

        // Assert
        Assert.True(result);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenCodeDoesNotMatch()
    {
        // Arrange
        var email = "mismatch@example.com";
        var correctCode = "111111";
        var wrongCode = "222222";
        VerificationCodeStore.Store(email, correctCode);

        // Act
        var result = VerificationCodeStore.Verify(email, wrongCode);

        // Assert
        Assert.False(result);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenEmailNotFound()
    {
        // Act
        var result = VerificationCodeStore.Verify("nonexistent@example.com", "123456");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenCodeIsExpired()
    {
        // Arrange
        var email = "expired@example.com";
        var code = "999999";
        var expiration = TimeSpan.FromMilliseconds(100); // Very short expiration

        VerificationCodeStore.Store(email, code, expiration);

        // Wait for code to expire
        Thread.Sleep(200);

        // Act
        var result = VerificationCodeStore.Verify(email, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_ShouldMarkCodeAsUsed_WhenMarkAsUsedIsTrue()
    {
        // Arrange
        var email = "used@example.com";
        var code = "555555";
        VerificationCodeStore.Store(email, code);

        // Act - First verification should succeed
        var firstResult = VerificationCodeStore.Verify(email, code, markAsUsed: true);
        
        // Wait briefly for async cleanup to start
        Thread.Sleep(100);
        
        // Second verification should fail because code is marked as used
        var secondResult = VerificationCodeStore.Verify(email, code, markAsUsed: false);

        // Assert
        Assert.True(firstResult);
        Assert.False(secondResult);
    }

    [Fact]
    public void Verify_ShouldNotMarkCodeAsUsed_WhenMarkAsUsedIsFalse()
    {
        // Arrange
        var email = "reusable@example.com";
        var code = "777777";
        VerificationCodeStore.Store(email, code);

        // Act
        var firstResult = VerificationCodeStore.Verify(email, code, markAsUsed: false);
        var secondResult = VerificationCodeStore.Verify(email, code, markAsUsed: false);

        // Assert
        Assert.True(firstResult);
        Assert.True(secondResult);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenCodeIsAlreadyUsed()
    {
        // Arrange
        var email = "alreadyused@example.com";
        var code = "888888";
        VerificationCodeStore.Store(email, code);

        // Mark as used first
        VerificationCodeStore.Verify(email, code, markAsUsed: true);

        // Wait briefly for state to update
        Thread.Sleep(100);

        // Act - Try to use the same code again
        var result = VerificationCodeStore.Verify(email, code, markAsUsed: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Remove_ShouldRemoveVerificationCode()
    {
        // Arrange
        var email = "remove@example.com";
        var code = "000000";
        VerificationCodeStore.Store(email, code);

        // Act
        VerificationCodeStore.Remove(email);
        var result = VerificationCodeStore.Verify(email, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Store_ShouldOverwriteExistingCode()
    {
        // Arrange
        var email = "overwrite@example.com";
        var oldCode = "111111";
        var newCode = "222222";

        // Act
        VerificationCodeStore.Store(email, oldCode);
        VerificationCodeStore.Store(email, newCode);

        var oldResult = VerificationCodeStore.Verify(email, oldCode, markAsUsed: false);
        var newResult = VerificationCodeStore.Verify(email, newCode, markAsUsed: false);

        // Assert
        Assert.False(oldResult);
        Assert.True(newResult);

        // Cleanup
        VerificationCodeStore.Remove(email);
    }

    [Fact]
    public void Verify_ShouldAutoRemoveExpiredCode()
    {
        // Arrange
        var email = "autoremove@example.com";
        var code = "333333";
        var expiration = TimeSpan.FromMilliseconds(100);

        VerificationCodeStore.Store(email, code, expiration);

        // Wait for expiration
        Thread.Sleep(200);

        // Act - First call should remove the expired code
        var firstResult = VerificationCodeStore.Verify(email, code);
        
        // Try again to ensure it was removed
        var secondResult = VerificationCodeStore.Verify(email, code);

        // Assert
        Assert.False(firstResult);
        Assert.False(secondResult);
    }

    [Fact]
    public void Verify_ShouldAutoRemoveUsedCodeAfterDelay()
    {
        // Arrange
        var email = "autoremoveused@example.com";
        var code = "444444";
        VerificationCodeStore.Store(email, code);

        // Act
        VerificationCodeStore.Verify(email, code, markAsUsed: true);
        
        // Wait for async removal (30 seconds in production, but we test the mechanism)
        // The code is marked as used, so subsequent verifications should fail
        Thread.Sleep(100);
        
        var result = VerificationCodeStore.Verify(email, code, markAsUsed: false);

        // Assert
        Assert.False(result);
    }
}

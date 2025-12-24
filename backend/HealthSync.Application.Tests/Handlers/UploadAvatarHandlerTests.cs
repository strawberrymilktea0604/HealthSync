using HealthSync.Application.Commands;
using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Moq;
using Xunit;

namespace HealthSync.Application.Tests.Handlers;

public class UploadAvatarHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
    private readonly UploadAvatarHandler _handler;

    public UploadAvatarHandlerTests()
    {
        _storageServiceMock = new Mock<IStorageService>();
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler = new UploadAvatarHandler(_storageServiceMock.Object, _userProfileRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUploadAndReturnAvatarUrl_WhenProfileExists()
    {
        // Arrange
        var profile = new UserProfile { UserId = 1, FullName = "Test User", AvatarUrl = null };
        using var stream = new MemoryStream();
        
        _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "avatar.jpg", "image/jpeg"))
            .ReturnsAsync("https://storage.com/avatars/avatar.jpg");
        _userProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(profile);
        _userProfileRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        var command = new UploadAvatarCommand
        {
            UserId = 1,
            FileStream = stream,
            FileName = "avatar.jpg",
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://storage.com/avatars/avatar.jpg", result);
        Assert.Equal("https://storage.com/avatars/avatar.jpg", profile.AvatarUrl);
        _storageServiceMock.Verify(s => s.UploadFileAsync(stream, "avatar.jpg", "image/jpeg"), Times.Once);
        _userProfileRepositoryMock.Verify(r => r.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldAvatar_WhenProfileHasExistingAvatar()
    {
        // Arrange
        var profile = new UserProfile 
        { 
            UserId = 1, 
            FullName = "Test User", 
            AvatarUrl = "https://storage.com/avatars/old-avatar.jpg" 
        };
        using var stream = new MemoryStream();
        
        _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "new-avatar.jpg", "image/jpeg"))
            .ReturnsAsync("https://storage.com/avatars/new-avatar.jpg");
        _storageServiceMock.Setup(s => s.DeleteFileAsync("old-avatar.jpg"))
            .ReturnsAsync(true);
        _userProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(profile);
        _userProfileRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        var command = new UploadAvatarCommand
        {
            UserId = 1,
            FileStream = stream,
            FileName = "new-avatar.jpg",
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://storage.com/avatars/new-avatar.jpg", result);
        _storageServiceMock.Verify(s => s.DeleteFileAsync("old-avatar.jpg"), Times.Once);
        _storageServiceMock.Verify(s => s.UploadFileAsync(stream, "new-avatar.jpg", "image/jpeg"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenDeletingOldAvatarFails()
    {
        // Arrange
        var profile = new UserProfile 
        { 
            UserId = 1, 
            FullName = "Test User", 
            AvatarUrl = "https://storage.com/avatars/old-avatar.jpg" 
        };
        using var stream = new MemoryStream();
        
        _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "new-avatar.jpg", "image/jpeg"))
            .ReturnsAsync("https://storage.com/avatars/new-avatar.jpg");
        _storageServiceMock.Setup(s => s.DeleteFileAsync("old-avatar.jpg"))
            .ThrowsAsync(new Exception("Deletion failed"));
        _userProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(profile);
        _userProfileRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        var command = new UploadAvatarCommand
        {
            UserId = 1,
            FileStream = stream,
            FileName = "new-avatar.jpg",
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should not throw, deletion errors are ignored
        Assert.Equal("https://storage.com/avatars/new-avatar.jpg", result);
        Assert.Equal("https://storage.com/avatars/new-avatar.jpg", profile.AvatarUrl);
    }

    [Fact]
    public async Task Handle_ShouldReturnUrl_WhenProfileDoesNotExist()
    {
        // Arrange
        using var stream = new MemoryStream();
        
        _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "avatar.jpg", "image/jpeg"))
            .ReturnsAsync("https://storage.com/avatars/avatar.jpg");
        _userProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync((UserProfile?)null);

        var command = new UploadAvatarCommand
        {
            UserId = 1,
            FileStream = stream,
            FileName = "avatar.jpg",
            ContentType = "image/jpeg"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://storage.com/avatars/avatar.jpg", result);
        _storageServiceMock.Verify(s => s.UploadFileAsync(stream, "avatar.jpg", "image/jpeg"), Times.Once);
        _userProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHandleDifferentContentTypes()
    {
        // Arrange
        var profile = new UserProfile { UserId = 1, FullName = "Test User" };
        using var stream = new MemoryStream();
        
        _storageServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), "avatar.png", "image/png"))
            .ReturnsAsync("https://storage.com/avatars/avatar.png");
        _userProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(1))
            .ReturnsAsync(profile);
        _userProfileRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UserProfile>()))
            .Returns(Task.CompletedTask);

        var command = new UploadAvatarCommand
        {
            UserId = 1,
            FileStream = stream,
            FileName = "avatar.png",
            ContentType = "image/png"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://storage.com/avatars/avatar.png", result);
        _storageServiceMock.Verify(s => s.UploadFileAsync(stream, "avatar.png", "image/png"), Times.Once);
    }
}

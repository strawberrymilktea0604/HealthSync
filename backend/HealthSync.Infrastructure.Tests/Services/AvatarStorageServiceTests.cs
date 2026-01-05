using HealthSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Moq;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Services;

public class AvatarStorageServiceTests
{
    private readonly Mock<IMinioClient> _minioClientMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public AvatarStorageServiceTests()
    {
        _minioClientMock = new Mock<IMinioClient>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["MinIO:PublicUrl"]).Returns("http://localhost:9002");

        // Common setup to avoid Constructor failure (sync-over-async .Wait())
        _minioClientMock
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Default to existing bucket to skip creation logic in ctor
    }

    // Constructor tests removed as bucket check logic was moved to UploadAvatarAsync


    [Fact]
    public async Task UploadAvatarAsync_ShouldUploadAndReturnUrl()
    {
        // Arrange
        var service = new AvatarStorageService(_minioClientMock.Object, _configurationMock.Object);
        var fileContent = new byte[] { 1, 2, 3 };
        using var stream = new MemoryStream(fileContent);
        
        // Mock PutObject
        _minioClientMock
            .Setup(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse(System.Net.HttpStatusCode.OK, "etag", new Dictionary<string, string>(), 0, "obj"));

        // Act
        var result = await service.UploadAvatarAsync(stream, "test.png", "image/png");

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("http://localhost:9002/avatars/", result);
        Assert.EndsWith("_test.png", result);
        
        _minioClientMock.Verify(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAvatarAsync_ShouldRemoveObject()
    {
        // Arrange
        var service = new AvatarStorageService(_minioClientMock.Object, _configurationMock.Object);
        
        // Act
        var result = await service.DeleteAvatarAsync("test.png");

        // Assert
        Assert.True(result);
        _minioClientMock.Verify(m => m.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAvatarAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        // Arrange
        var service = new AvatarStorageService(_minioClientMock.Object, _configurationMock.Object);
        
        _minioClientMock
            .Setup(m => m.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("MinIO offline"));

        // Act
        var result = await service.DeleteAvatarAsync("test.png");

        // Assert
        Assert.False(result);
    }
}

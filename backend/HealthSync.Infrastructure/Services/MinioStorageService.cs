using HealthSync.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace HealthSync.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _endpoint;

    public MinioStorageService(IConfiguration configuration)
    {
        var minioEndpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");
        
        _bucketName = configuration["MinIO:BucketName"] ?? "healthsync";
        // Note: http is used for local development; in production, useSSL should be true for https
        _endpoint = useSSL ? $"https://{minioEndpoint}" : $"http://{minioEndpoint}";

        _minioClient = new MinioClient()
            .WithEndpoint(minioEndpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSSL)
            .Build();

        EnsureBucketExists().Wait();
    }

    private async Task EnsureBucketExists()
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);
            
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);
                
                await _minioClient.MakeBucketAsync(mbArgs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring bucket exists: {ex.Message}");
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            // Return the public URL
            return $"{_endpoint}/{_bucketName}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading file to MinIO: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file from MinIO: {ex.Message}");
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithExpiry(60 * 60 * 24); // 24 hours

            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting file URL from MinIO: {ex.Message}", ex);
        }
    }
}

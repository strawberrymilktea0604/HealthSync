using HealthSync.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace HealthSync.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public MinioStorageService(IConfiguration configuration)
    {
        var minioEndpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");
        
        _bucketName = configuration["MinIO:BucketName"] ?? "healthsync-files";
        // Note: http is used for local development; in production, useSSL should be true for https
        var endpoint = useSSL ? $"https://{minioEndpoint}" : $"http://{minioEndpoint}";
        // Public URL for frontend access (e.g., http://localhost:9002 when MinIO is accessed from browser)
        _publicUrl = configuration["MinIO:PublicUrl"] ?? endpoint;

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
                
                // Set public read policy for the bucket
                var policyJson = @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Principal"": { ""AWS"": [""*""] },
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::" + _bucketName + @"/*""]
                    }]
                }";
                
                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(_bucketName)
                    .WithPolicy(policyJson);
                    
                await _minioClient.SetPolicyAsync(setPolicyArgs);
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
            // Don't add GUID for files that already include folder paths (e.g., "exercises/1_image.jpg")
            var objectName = fileName.Contains("/") ? fileName : $"{Guid.NewGuid()}_{fileName}";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            // Return the public URL (use publicUrl for frontend access)
            return $"{_publicUrl}/{_bucketName}/{objectName}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error uploading file to MinIO: {ex.Message}", ex);
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
            throw new InvalidOperationException($"Error getting file URL from MinIO: {ex.Message}", ex);
        }
    }
}

using HealthSync.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace HealthSync.Infrastructure.Services;

public class AvatarStorageService : IAvatarStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _publicUrl;
    private const string AVATAR_BUCKET = "avatars";
    private const string DEFAULT_MINIO_URL = "http://localhost:9002";

    public AvatarStorageService(IMinioClient minioClient, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _publicUrl = configuration["MinIO:PublicUrl"] ?? DEFAULT_MINIO_URL;
        EnsureAvatarBucketExistsAsync().Wait();
    }

    private async Task EnsureAvatarBucketExistsAsync()
    {
        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(AVATAR_BUCKET);
            bool found = await _minioClient.BucketExistsAsync(beArgs);

            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(AVATAR_BUCKET));
                
                // Set public read policy
                var policyJson = @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Principal"": { ""AWS"": [""*""] },
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::" + AVATAR_BUCKET + @"/*""]
                    }]
                }";
                
                await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                    .WithBucket(AVATAR_BUCKET)
                    .WithPolicy(policyJson));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring avatar bucket exists: {ex.Message}");
        }
    }

    public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}_{fileName}";
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(AVATAR_BUCKET)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            // Return the public URL
            return $"{_publicUrl}/{AVATAR_BUCKET}/{objectName}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error uploading avatar to MinIO: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAvatarAsync(string fileName)
    {
        try
        {
            var rmArgs = new RemoveObjectArgs()
                .WithBucket(AVATAR_BUCKET)
                .WithObject(fileName);
            
            await _minioClient.RemoveObjectAsync(rmArgs);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting avatar from MinIO: {ex.Message}");
            return false;
        }
    }
}

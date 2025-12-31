namespace HealthSync.Domain.Interfaces;

public interface IAvatarStorageService
{
    Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAvatarAsync(string fileName);
}

using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Commands;

public class UploadAvatarHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IStorageService _storageService;
    private readonly IUserProfileRepository _userProfileRepository;

    public UploadAvatarHandler(IStorageService storageService, IUserProfileRepository userProfileRepository)
    {
        _storageService = storageService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        // Upload file to MinIO
        var avatarUrl = await _storageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType);

        // Update user profile with new avatar URL
        var profile = await _userProfileRepository.GetByUserIdAsync(request.UserId);
        if (profile != null)
        {
            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(profile.AvatarUrl))
            {
                try
                {
                    var oldFileName = profile.AvatarUrl.Split('/')[^1];
                    await _storageService.DeleteFileAsync(oldFileName);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }

            profile.AvatarUrl = avatarUrl;
            await _userProfileRepository.UpdateAsync(profile);
        }

        return avatarUrl;
    }
}

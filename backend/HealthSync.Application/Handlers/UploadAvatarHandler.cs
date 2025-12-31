using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Commands;

public class UploadAvatarHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IAvatarStorageService _avatarStorageService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IApplicationDbContext _context;

    public UploadAvatarHandler(
        IAvatarStorageService avatarStorageService, 
        IUserProfileRepository userProfileRepository,
        IApplicationDbContext context)
    {
        _avatarStorageService = avatarStorageService;
        _userProfileRepository = userProfileRepository;
        _context = context;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        // Upload file to avatars bucket
        var avatarUrl = await _avatarStorageService.UploadAvatarAsync(request.FileStream, request.FileName, request.ContentType);

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
                    await _avatarStorageService.DeleteAvatarAsync(oldFileName);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }

            profile.AvatarUrl = avatarUrl;
            await _userProfileRepository.UpdateAsync(profile);
            
            // FIX: Cập nhật luôn ApplicationUser.AvatarUrl để đồng bộ 2 bảng
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
            if (user != null)
            {
                user.AvatarUrl = avatarUrl;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return avatarUrl;
    }
}

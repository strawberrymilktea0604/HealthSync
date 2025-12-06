using MediatR;

namespace HealthSync.Application.Commands;

public class UploadAvatarCommand : IRequest<string>
{
    public int UserId { get; set; }
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class GetGoogleAndroidClientIdQueryHandler : IRequestHandler<GetGoogleAndroidClientIdQuery, string>
{
    private readonly IGoogleAuthService _googleAuthService;

    public GetGoogleAndroidClientIdQueryHandler(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    public async Task<string> Handle(GetGoogleAndroidClientIdQuery request, CancellationToken cancellationToken)
    {
        var androidClientId = await _googleAuthService.GetAndroidClientIdAsync();
        return androidClientId;
    }
}

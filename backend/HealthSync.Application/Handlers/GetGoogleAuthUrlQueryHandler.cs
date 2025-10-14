using HealthSync.Application.Queries;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class GetGoogleAuthUrlQueryHandler : IRequestHandler<GetGoogleAuthUrlQuery, string>
{
    private readonly IGoogleAuthService _googleAuthService;

    public GetGoogleAuthUrlQueryHandler(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }

    public async Task<string> Handle(GetGoogleAuthUrlQuery request, CancellationToken cancellationToken)
    {
        return await _googleAuthService.GetAuthorizationUrl(request.State);
    }
}
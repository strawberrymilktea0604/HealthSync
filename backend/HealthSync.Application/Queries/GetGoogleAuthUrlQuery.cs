using MediatR;

namespace HealthSync.Application.Queries;

public class GetGoogleAuthUrlQuery : IRequest<string>
{
    public string State { get; set; } = string.Empty;
}
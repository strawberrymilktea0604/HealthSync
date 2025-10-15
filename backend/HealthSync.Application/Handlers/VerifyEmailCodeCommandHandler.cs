using HealthSync.Application.Commands;
using HealthSync.Application.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HealthSync.Application.Handlers;

public class VerifyEmailCodeCommandHandler : IRequestHandler<VerifyEmailCodeCommand, bool>
{
    public Task<bool> Handle(VerifyEmailCodeCommand request, CancellationToken cancellationToken)
    {
        var isValid = VerificationCodeStore.Verify(request.Email, request.Code);
        return Task.FromResult(isValid);
    }
}
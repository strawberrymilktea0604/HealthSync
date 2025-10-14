using HealthSync.Application.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HealthSync.Application.Handlers;

public class VerifyEmailCodeCommandHandler : IRequestHandler<VerifyEmailCodeCommand, bool>
{
    private static readonly Dictionary<string, string> _verificationCodes = new();

    public async Task<bool> Handle(VerifyEmailCodeCommand request, CancellationToken cancellationToken)
    {
        if (_verificationCodes.TryGetValue(request.Email, out var storedCode))
        {
            if (storedCode == request.Code)
            {
                // Remove the code after successful verification
                _verificationCodes.Remove(request.Email);
                return true;
            }
        }
        return false;
    }
}
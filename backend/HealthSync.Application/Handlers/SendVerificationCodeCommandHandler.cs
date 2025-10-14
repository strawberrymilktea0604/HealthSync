using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthSync.Application.Handlers;

public class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommand, Unit>
{
    private readonly IEmailService _emailService;
    private static readonly Dictionary<string, string> _verificationCodes = new();

    public SendVerificationCodeCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<Unit> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        // Generate a 6-digit code
        var code = new Random().Next(100000, 999999).ToString();

        // Store the code (in production, use cache/database)
        _verificationCodes[request.Email] = code;

        // Send email
        await _emailService.SendVerificationCodeAsync(request.Email, code);

        return Unit.Value;
    }
}
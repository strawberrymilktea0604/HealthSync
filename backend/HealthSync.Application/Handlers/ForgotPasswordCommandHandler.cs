using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using HealthSync.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Don't reveal if email exists or not for security
            return;
        }

        var resetToken = await _jwtTokenService.GenerateResetTokenAsync(user.UserId, user.Email);
        await _emailService.SendResetPasswordEmailAsync(user.Email, resetToken);
    }
}
using HealthSync.Application.Commands;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthSync.Application.Handlers;

public class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;

    public SendVerificationCodeCommandHandler(IEmailService emailService, IApplicationDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    public async Task<Unit> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists in database
        var existingUser = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email đã tồn tại trong hệ thống!");
        }

        // Generate a 6-digit code
        var code = new Random().Next(100000, 999999).ToString();

        // Store the code with 5 minutes expiration
        VerificationCodeStore.Store(request.Email, code, TimeSpan.FromMinutes(5));

        // Send email
        await _emailService.SendVerificationCodeAsync(request.Email, code);

        return Unit.Value;
    }
}
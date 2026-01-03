using HealthSync.Application.Commands;
using HealthSync.Application.DTOs;
using HealthSync.Application.Services;
using HealthSync.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Application.Handlers;

public class VerifyResetOtpCommandHandler : IRequestHandler<VerifyResetOtpCommand, VerifyResetOtpResponse>
{
    private readonly IOtpService _otpService;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public VerifyResetOtpCommandHandler(
        IOtpService otpService,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _otpService = otpService;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<VerifyResetOtpResponse> Handle(VerifyResetOtpCommand request, CancellationToken cancellationToken)
    {
        var isValid = _otpService.ValidateOtp(request.Email, request.Otp);
        
        if (!isValid)
        {
            return new VerifyResetOtpResponse
            {
                Success = false,
                Message = "Mã OTP không hợp lệ hoặc đã hết hạn."
            };
        }

        // Generate Reset Token
        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);

        if (user == null)
        {
             // Should not happen if flow is correct, but safe guard
             return new VerifyResetOtpResponse
             {
                 Success = false,
                 Message = "Người dùng không tồn tại."
             };
        }

        // Generate Reset Token
        var resetToken = await _jwtTokenService.GenerateResetTokenAsync(user.UserId, user.Email);

        return new VerifyResetOtpResponse
        {
            Success = true,
            Token = resetToken,
            Message = "OTP verified successfully."
        };
    }
}
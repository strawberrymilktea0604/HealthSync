using HealthSync.Application.Commands;
using HealthSync.Domain.Interfaces;
using MediatR;

namespace HealthSync.Application.Handlers;

public class UpdateUserPasswordCommandHandler : IRequestHandler<UpdateUserPasswordCommand, bool>
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly IAuthService _authService;

    public UpdateUserPasswordCommandHandler(
        IApplicationUserRepository userRepository,
        IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<bool> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return false;
        }

        // Hash new password
        user.PasswordHash = _authService.HashPassword(request.NewPassword);
        
        await _userRepository.UpdateAsync(user);
        return true;
    }
}

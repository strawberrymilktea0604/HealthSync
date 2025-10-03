using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;

namespace HealthSync.Application.Services;

public class ApplicationUserService : IApplicationUserService
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly IAuthService _authService;

    public ApplicationUserService(IApplicationUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<ApplicationUser> CreateUserAsync(string email, string password, string role = "Customer")
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var hashedPassword = _authService.HashPassword(password);
        var user = new ApplicationUser
        {
            Email = email,
            PasswordHash = hashedPassword,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task UpdateUserAsync(ApplicationUser user)
    {
        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        await _userRepository.DeleteAsync(user);
    }

    public async Task<bool> AuthenticateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return _authService.VerifyPassword(password, user.PasswordHash);
    }
}
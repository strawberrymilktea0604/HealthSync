using HealthSync.Domain.Entities;

namespace HealthSync.Domain.Interfaces;

public interface IApplicationUserService
{
    Task<ApplicationUser?> GetUserByIdAsync(int id);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser> CreateUserAsync(string email, string password, string role = "Customer");
    Task UpdateUserAsync(ApplicationUser user);
    Task DeleteUserAsync(int id);
    Task<bool> AuthenticateUserAsync(string email, string password);
}
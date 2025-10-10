using HealthSync.Domain.Entities;

namespace HealthSync.Domain.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(UserProfile profile);
}
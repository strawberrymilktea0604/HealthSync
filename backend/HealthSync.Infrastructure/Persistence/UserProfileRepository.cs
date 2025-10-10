using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Persistence;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IApplicationDbContext _context;

    public UserProfileRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(int userId)
    {
        return await _context.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task AddAsync(UserProfile profile)
    {
        _context.Add(profile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        _context.Update(profile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserProfile profile)
    {
        _context.Remove(profile);
        await _context.SaveChangesAsync();
    }
}
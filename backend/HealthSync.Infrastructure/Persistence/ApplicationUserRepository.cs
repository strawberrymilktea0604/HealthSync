using HealthSync.Domain.Entities;
using HealthSync.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Persistence;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly IApplicationDbContext _context;

    public ApplicationUserRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id)
    {
        return await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.WorkoutLogs)
            .Include(u => u.NutritionLogs)
            .Include(u => u.Goals)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.ApplicationUsers
            .Include(u => u.Profile)
            .Include(u => u.WorkoutLogs)
            .Include(u => u.NutritionLogs)
            .Include(u => u.Goals)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _context.ApplicationUsers
            .Include(u => u.Profile)
            .ToListAsync();
    }

    public async Task AddAsync(ApplicationUser user)
    {
        _context.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApplicationUser user)
    {
        _context.Remove(user);
        await _context.SaveChangesAsync();
    }
}
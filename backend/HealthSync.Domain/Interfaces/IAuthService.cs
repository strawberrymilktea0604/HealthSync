using HealthSync.Domain.Entities;

namespace HealthSync.Domain.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateJwtToken(ApplicationUser user);
}

public interface IApplicationDbContext
{
    IQueryable<ApplicationUser> ApplicationUsers { get; }
    IQueryable<UserProfile> UserProfiles { get; }
    IQueryable<Goal> Goals { get; }
    IQueryable<ProgressRecord> ProgressRecords { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
}
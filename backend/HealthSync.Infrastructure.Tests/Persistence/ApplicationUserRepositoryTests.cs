using HealthSync.Domain.Entities;
using HealthSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthSync.Infrastructure.Tests.Persistence;

public class ApplicationUserRepositoryTests
{
    private readonly HealthSyncDbContext _context;
    private readonly ApplicationUserRepository _repository;

    public ApplicationUserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<HealthSyncDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HealthSyncDbContext(options);
        _repository = new ApplicationUserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var user = new ApplicationUser 
        { 
            Email = "test@example.com", 
            UserName = "test"
        };

        await _repository.AddAsync(user);

        var savedUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(savedUser);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WithIncludes()
    {
        var user = new ApplicationUser
        {
            Email = "include@example.com",
            UserName = "include",
            Profile = new UserProfile { FullName = "Include Test" },
            Goals = new List<Goal> { new Goal { Type = "Weight Loss" } }
        };

        _context.ApplicationUsers.Add(user);
        await _context.SaveChangesAsync();

        var fetchedUser = await _repository.GetByIdAsync(user.UserId);

        Assert.NotNull(fetchedUser);
        Assert.NotNull(fetchedUser.Profile);
        Assert.Equal("Include Test", fetchedUser.Profile.FullName);
        Assert.NotEmpty(fetchedUser.Goals);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        var user = new ApplicationUser
        {
            Email = "email@example.com",
            UserName = "email"
        };

        _context.ApplicationUsers.Add(user);
        await _context.SaveChangesAsync();

        var fetchedUser = await _repository.GetByEmailAsync("email@example.com");

        Assert.NotNull(fetchedUser);
        Assert.Equal("email", fetchedUser.UserName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        _context.ApplicationUsers.Add(new ApplicationUser { Email = "1@test.com", UserName = "1" });
        _context.ApplicationUsers.Add(new ApplicationUser { Email = "2@test.com", UserName = "2" });
        await _context.SaveChangesAsync();

        var users = await _repository.GetAllAsync();

        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var user = new ApplicationUser { Email = "update@test.com", UserName = "update" };
        _context.ApplicationUsers.Add(user);
        await _context.SaveChangesAsync();

        user.UserName = "updated_name";
        await _repository.UpdateAsync(user);

        var updatedUser = await _context.ApplicationUsers.FindAsync(user.UserId);
        Assert.Equal("updated_name", updatedUser!.UserName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var user = new ApplicationUser { Email = "delete@test.com", UserName = "delete" };
        _context.ApplicationUsers.Add(user);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(user);

        var deletedUser = await _context.ApplicationUsers.FindAsync(user.UserId);
        Assert.Null(deletedUser);
    }
}

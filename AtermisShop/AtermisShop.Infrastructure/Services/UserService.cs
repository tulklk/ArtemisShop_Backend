using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using AtermisShop.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtermisShop.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(IApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<ApplicationUser?> FindByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return Task.FromResult(PasswordHasher.VerifyPassword(password, user.PasswordHash));
    }

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, string password)
    {
        user.Id = Guid.NewGuid();
        user.PasswordHash = PasswordHasher.HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await FindByIdAsync(userId);
        if (user == null)
            return false;

        // Verify current password
        var currentPasswordValid = await CheckPasswordAsync(user, currentPassword);
        if (!currentPasswordValid)
            return false;

        // Update password
        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        await UpdateAsync(user);
        
        return true;
    }

    public Task<bool> IsAdminAsync(ApplicationUser user)
    {
        // Role 1 = Admin (you can adjust this based on your role enum)
        return Task.FromResult(user.Role == 1);
    }
}


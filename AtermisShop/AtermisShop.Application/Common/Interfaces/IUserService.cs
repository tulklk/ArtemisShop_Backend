using AtermisShop.Domain.Users;

namespace AtermisShop.Application.Common.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(Guid id);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<ApplicationUser> CreateAsync(ApplicationUser user, string password);
    Task UpdateAsync(ApplicationUser user);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> IsAdminAsync(ApplicationUser user);
}


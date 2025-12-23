using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Users;
using AtermisShop.Infrastructure.Auth;

namespace AtermisShop.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAdminUserAsync(IUserService userService)
    {
        // Kiểm tra xem đã có admin user chưa
        var adminEmail = "admin@artemisshop.com";
        var adminPassword = "Admin@123456";
        var adminUser = await userService.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Tạo admin user với Role = 1 (Admin)
            adminUser = new ApplicationUser
            {
                Email = adminEmail,
                FullName = "Administrator",
                EmailVerified = true,
                Role = 1, // 1 = Admin
                IsActive = true
            };

            await userService.CreateAsync(adminUser, adminPassword); // Password mặc định
            Console.WriteLine($"Admin user created: {adminEmail} / Password: {adminPassword}");
        }
        else
        {
            // Đảm bảo admin user có Role = 1, email verified, và password hash đúng format
            var needsUpdate = false;
            
            if (adminUser.Role != 1)
            {
                adminUser.Role = 1;
                needsUpdate = true;
            }
            
            if (!adminUser.EmailVerified)
            {
                adminUser.EmailVerified = true;
                needsUpdate = true;
            }
            
            // Luôn reset password hash để đảm bảo dùng format mới (SHA256)
            // Vì có thể admin user có password hash từ Identity (format cũ)
            var correctPasswordHash = PasswordHasher.HashPassword(adminPassword);
            if (adminUser.PasswordHash != correctPasswordHash)
            {
                adminUser.PasswordHash = correctPasswordHash;
                needsUpdate = true;
                Console.WriteLine($"Admin user password hash reset to new format: {adminEmail} / Password: {adminPassword}");
            }
            
            if (needsUpdate)
            {
                await userService.UpdateAsync(adminUser);
                Console.WriteLine($"Admin user updated: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Admin user already exists with correct settings: {adminEmail}");
            }
        }
    }
}


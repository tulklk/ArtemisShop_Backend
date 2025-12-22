using AtermisShop.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace AtermisShop.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Tạo Admin role nếu chưa có
        var adminRoleName = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRoleName) { Id = Guid.NewGuid() });
        }

        // Kiểm tra xem đã có admin user chưa
        var adminEmail = "admin@artemisshop.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Tạo admin user
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true, // Bỏ qua email confirmation cho admin
                EmailVerified = true,
                FullName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456"); // Password mặc định
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
                Console.WriteLine($"Admin user created: {adminEmail} / Password: Admin@123456");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Đảm bảo admin user có role Admin
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
            Console.WriteLine($"Admin user already exists: {adminEmail}");
        }
    }
}


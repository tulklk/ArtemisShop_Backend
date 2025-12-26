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

    public static async Task SeedCustomerUserAsync(IUserService userService)
    {
        // Tạo customer user mẫu với email đã verified
        var customerEmail = "customer@example.com";
        var customerPassword = "Customer@123456";
        var customerUser = await userService.FindByEmailAsync(customerEmail);

        if (customerUser == null)
        {
            // Tạo customer user với Role = 0 (Customer)
            customerUser = new ApplicationUser
            {
                Email = customerEmail,
                FullName = "Nguyễn Văn Customer",
                PhoneNumber = "0901234567",
                EmailVerified = true, // Email đã được verified
                Role = 0, // 0 = Customer
                IsActive = true
            };

            await userService.CreateAsync(customerUser, customerPassword);
            Console.WriteLine($"Customer user created: {customerEmail} / Password: {customerPassword}");
        }
        else
        {
            // Đảm bảo customer user có email verified và password đúng
            var needsUpdate = false;
            
            if (!customerUser.EmailVerified)
            {
                customerUser.EmailVerified = true;
                needsUpdate = true;
            }
            
            if (customerUser.Role != 0)
            {
                customerUser.Role = 0;
                needsUpdate = true;
            }
            
            // Reset password hash để đảm bảo dùng format mới
            var correctPasswordHash = PasswordHasher.HashPassword(customerPassword);
            if (customerUser.PasswordHash != correctPasswordHash)
            {
                customerUser.PasswordHash = correctPasswordHash;
                needsUpdate = true;
                Console.WriteLine($"Customer user password hash reset: {customerEmail} / Password: {customerPassword}");
            }
            
            if (needsUpdate)
            {
                await userService.UpdateAsync(customerUser);
                Console.WriteLine($"Customer user updated: {customerEmail}");
            }
            else
            {
                Console.WriteLine($"Customer user already exists with correct settings: {customerEmail}");
            }
        }
    }
}


using AtermisShop.Domain.Chat;
using AtermisShop.Domain.News;
using AtermisShop.Domain.Orders;
using AtermisShop.Domain.Products;
using AtermisShop.Domain.Users;
using AtermisShop.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    // Products
    DbSet<Product> Products { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductSpecification> ProductSpecifications { get; }
    DbSet<ProductReview> ProductReviews { get; }
    DbSet<ProductComment> ProductComments { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }

    // Orders
    DbSet<Domain.Orders.Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<VoucherUsage> VoucherUsages { get; }
    DbSet<Payment> Payments { get; }

    // News
    DbSet<NewsPost> NewsPosts { get; }

    // Wishlist
    DbSet<Domain.Wishlist.Wishlist> Wishlists { get; }

    // Users
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }

    // Chat
    DbSet<ChatMessage> ChatMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}



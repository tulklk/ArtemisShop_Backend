using AtermisShop.Application.Common.Interfaces;
using AtermisShop.Domain.Chat;
using AtermisShop.Domain.News;
using AtermisShop.Domain.Orders;
using AtermisShop.Domain.Products;
using AtermisShop.Domain.Users;
using AtermisShop.Domain.Wishlist;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AtermisShop.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Đổi tên các bảng Identity từ AspNet* thành tên không có prefix
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            
            // Set default values
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.EmailVerified).HasDefaultValue(false);
        });
        
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Đổi tên các bảng theo schema mới
        builder.Entity<ProductCategory>().ToTable("ProductCategories");
        builder.Entity<NewsPost>().ToTable("NewsPosts");
        builder.Entity<Wishlist>().ToTable("Wishlists");
    }

    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductComment> ProductComments => Set<ProductComment>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // Orders
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
    public DbSet<Payment> Payments => Set<Payment>();

    // News
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();

    // Wishlist
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();

    // Users
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    // Chat
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Đồng bộ EmailConfirmed và EmailVerified
        var entries = ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var user = entry.Entity;
            
            // Đồng bộ EmailVerified với EmailConfirmed
            if (entry.Property(nameof(ApplicationUser.EmailConfirmed)).IsModified)
            {
                user.EmailVerified = user.EmailConfirmed;
            }
            else if (entry.Property(nameof(ApplicationUser.EmailVerified)).IsModified)
            {
                user.EmailConfirmed = user.EmailVerified;
            }
            
            // Set UpdatedAt khi có thay đổi
            if (entry.State == EntityState.Modified)
            {
                user.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Added)
            {
                user.CreatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }
}



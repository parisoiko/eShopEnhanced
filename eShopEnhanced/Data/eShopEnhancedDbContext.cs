using eShopEnhanced.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Data
{
    // IdentityDbContext = DbContext + the tables ASP.NET Core Identity needs
    // (AspNetUsers, AspNetRoles, AspNetUserRoles, ...). It's still our one shop database.
    // Identity configures those tables in base.OnModelCreating(...), which we already call.
    // <ApplicationUser>: use our own user class (with name and address) instead of IdentityUser.
    public class eShopEnhancedDbContext : IdentityDbContext<ApplicationUser>
    {
        public eShopEnhancedDbContext(DbContextOptions<eShopEnhancedDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // decimal(18,2): up to 18 digits, 2 after the decimal point (e.g. 15.95)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);

            // The extra user columns: same sizes as the matching Order columns
            modelBuilder.Entity<ApplicationUser>(user =>
            {
                user.Property(u => u.FirstName).HasMaxLength(50);
                user.Property(u => u.LastName).HasMaxLength(50);
                user.Property(u => u.AddressLine1).HasMaxLength(100);
                user.Property(u => u.AddressLine2).HasMaxLength(100);
                user.Property(u => u.ZipCode).HasMaxLength(10);
                user.Property(u => u.City).HasMaxLength(50);
                user.Property(u => u.State).HasMaxLength(50);
                user.Property(u => u.Country).HasMaxLength(50);
                user.Ignore(u => u.Initials);   // calculated, not stored
            });
        }
    }
}

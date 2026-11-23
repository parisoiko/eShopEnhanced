using eShopEnhanced.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Data
{
    // Shop tables and ASP.NET Core Identity tables in one database, with ApplicationUser as the user type.
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

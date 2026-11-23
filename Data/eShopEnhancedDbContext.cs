using eShopEnhanced.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Data
{
    public class eShopEnhancedDbContext : DbContext
    {
        public eShopEnhancedDbContext(DbContextOptions<eShopEnhancedDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // decimal(18,2): up to 18 digits, 2 after the decimal point (e.g. 15.95)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}

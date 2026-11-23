using eShopEnhanced.Data;
using eShopEnhanced.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public ProductRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        // Base for every product query: read-only, with subcategory and category loaded in the same query
        private IQueryable<Product> ProductsWithCategories =>
            _eShopEnhancedDbContext.Products
                .AsNoTracking()
                .Include(p => p.SubCategory)
                .ThenInclude(s => s.Category);

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await ProductsWithCategories
                .OrderBy(p => p.SubCategory.Category.CategoryName)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Product>> GetPicksOfTheWeekAsync()
        {
            return await ProductsWithCategories
                .Where(p => p.IsPickOfTheWeek)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string categoryName)
        {
            return await ProductsWithCategories
                .Where(p => p.SubCategory.Category.CategoryName == categoryName)
                .OrderBy(p => p.SubCategory.SubCategoryName)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        // Matches name, short description, subcategory or category. Contains() translates to
        // LIKE '%...%', which is case-insensitive under SQL Server's default collation.
        public async Task<List<Product>> SearchProductsAsync(string searchQuery)
        {
            string query = searchQuery.Trim();

            return await ProductsWithCategories
                .Where(p => p.Name.Contains(query)
                         || (p.ShortDescription != null && p.ShortDescription.Contains(query))
                         || p.SubCategory.SubCategoryName.Contains(query)
                         || p.SubCategory.Category.CategoryName.Contains(query))
                .OrderBy(p => p.Name)
                .Take(24)   // never return the whole shop for a one-letter search
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await ProductsWithCategories
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
    }
}

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

        // The starting point for every product query:
        // - AsNoTracking: we only read, so EF doesn't need to watch the objects for changes
        // - Include/ThenInclude: load the subcategory and its category in the same SQL query
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
            // The Where becomes a SQL WHERE, so only this category's products are loaded
            return await ProductsWithCategories
                .Where(p => p.SubCategory.Category.CategoryName == categoryName)
                .OrderBy(p => p.SubCategory.SubCategoryName)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        // Finds products whose name, short description, subcategory or category contains the text.
        // Contains(...) becomes SQL LIKE '%text%', and SQL Server ignores upper/lower case,
        // so "CAKE", "cake" and "Cake" all match.
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

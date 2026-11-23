using eShopEnhanced.Data;
using eShopEnhanced.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public CategoryRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _eShopEnhancedDbContext.Categories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            // This comparison runs in SQL Server, which ignores upper/lower case by default,
            // and only the one matching row is loaded.
            return await _eShopEnhancedDbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName);
        }
    }
}

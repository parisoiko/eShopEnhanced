using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Models
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public CategoryRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public IEnumerable<Category> AllCategories =>
            _eShopEnhancedDbContext.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName);
    }
}

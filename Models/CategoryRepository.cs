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
            _eShopEnhancedDbContext.Categories.OrderBy(c => c.CategoryName);
    }
}

using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Models
{
    public class SubCategoryRepository : ISubCategoryRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public SubCategoryRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public IEnumerable<SubCategory> AllSubCategories =>
            _eShopEnhancedDbContext.SubCategories
                .Include(s => s.Category)
                .OrderBy(s => s.SubCategoryName);
    }
}

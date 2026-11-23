using eShopEnhanced.Data;
using eShopEnhanced.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Repositories
{
    public class SubCategoryRepository : ISubCategoryRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public SubCategoryRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public async Task<List<SubCategory>> GetAllSubCategoriesAsync()
        {
            return await _eShopEnhancedDbContext.SubCategories
                .AsNoTracking()
                .Include(s => s.Category)
                .OrderBy(s => s.SubCategoryName)
                .ToListAsync();
        }
    }
}

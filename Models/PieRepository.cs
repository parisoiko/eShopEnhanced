using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Models
{
    public class PieRepository : IPieRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public PieRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public IEnumerable<Pie> AllPies
        {
            get
            {
                return _eShopEnhancedDbContext.Pies
                    .Include(p => p.SubCategory).ThenInclude(s => s.Category)
                    .OrderBy(p => p.SubCategory.Category.CategoryName)
                    .ThenBy(p => p.Name);
            }
        }

        public IEnumerable<Pie> PiesOfTheWeek
        {
            get
            {
                return _eShopEnhancedDbContext.Pies.Include(p => p.SubCategory).ThenInclude(s => s.Category).Where(p => p.IsPieOfTheWeek);
            }
        }

        public IEnumerable<Pie> PiesByCategory(string categoryName)
        {
            // The Where runs in the database (SQL WHERE), so only this category's products are loaded
            return _eShopEnhancedDbContext.Pies
                .Include(p => p.SubCategory).ThenInclude(s => s.Category)
                .Where(p => p.SubCategory.Category.CategoryName == categoryName)
                .OrderBy(p => p.SubCategory.SubCategoryName)
                .ThenBy(p => p.Name);
        }

        public Pie? GetPieById(int pieId)
        {
            return _eShopEnhancedDbContext.Pies.Include(p => p.SubCategory).ThenInclude(s => s.Category).FirstOrDefault(p => p.PieId == pieId);
        }
    }
}

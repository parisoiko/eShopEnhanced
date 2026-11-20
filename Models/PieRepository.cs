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
                return _eShopEnhancedDbContext.Pies.Include(p => p.SubCategory).ThenInclude(s => s.Category);
            }
        }

        public IEnumerable<Pie> PiesOfTheWeek
        {
            get
            {
                return _eShopEnhancedDbContext.Pies.Include(p => p.SubCategory).ThenInclude(s => s.Category).Where(p => p.IsPieOfTheWeek);
            }
        }

        public Pie? GetPieById(int pieId)
        {
            return _eShopEnhancedDbContext.Pies.Include(p => p.SubCategory).ThenInclude(s => s.Category).FirstOrDefault(p => p.PieId == pieId);
        }
    }
}

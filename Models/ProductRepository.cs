using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Models
{
    public class ProductRepository : IProductRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public ProductRepository(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public IEnumerable<Product> AllProducts
        {
            get
            {
                return _eShopEnhancedDbContext.Products
                    .Include(p => p.SubCategory).ThenInclude(s => s.Category)
                    .OrderBy(p => p.SubCategory.Category.CategoryName)
                    .ThenBy(p => p.Name);
            }
        }

        public IEnumerable<Product> PicksOfTheWeek
        {
            get
            {
                return _eShopEnhancedDbContext.Products.Include(p => p.SubCategory).ThenInclude(s => s.Category).Where(p => p.IsPickOfTheWeek);
            }
        }

        public IEnumerable<Product> ProductsByCategory(string categoryName)
        {
            // The Where runs in the database (SQL WHERE), so only this category's products are loaded
            return _eShopEnhancedDbContext.Products
                .Include(p => p.SubCategory).ThenInclude(s => s.Category)
                .Where(p => p.SubCategory.Category.CategoryName == categoryName)
                .OrderBy(p => p.SubCategory.SubCategoryName)
                .ThenBy(p => p.Name);
        }

        public Product? GetProductById(int productId)
        {
            return _eShopEnhancedDbContext.Products.Include(p => p.SubCategory).ThenInclude(s => s.Category).FirstOrDefault(p => p.ProductId == productId);
        }
    }
}

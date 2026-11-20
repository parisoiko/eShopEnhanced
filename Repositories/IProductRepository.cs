using eShopEnhanced.Models;
namespace eShopEnhanced.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> AllProducts { get; }
        IEnumerable<Product> PicksOfTheWeek { get; }
        IEnumerable<Product> ProductsByCategory(string categoryName);
        Product? GetProductById(int productId);
    }
}

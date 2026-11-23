using eShopEnhanced.Models;

namespace eShopEnhanced.Repositories
{
    // Every method runs its query right away and returns a finished list,
    // so a query never runs twice or while a view is being rendered.
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> GetPicksOfTheWeekAsync();
        Task<List<Product>> GetProductsByCategoryAsync(string categoryName);
        Task<Product?> GetProductByIdAsync(int productId);
    }
}

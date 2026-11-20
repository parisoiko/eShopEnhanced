namespace eShopEnhanced.Models
{
    public interface IProductRepository
    {
        IEnumerable<Product> AllProducts { get; }
        IEnumerable<Product> PicksOfTheWeek { get; }
        IEnumerable<Product> ProductsByCategory(string categoryName);
        Product? GetProductById(int productId);
    }
}

using eShopEnhanced.Models;
namespace eShopEnhanced.Services
{
    public interface IShoppingCart
    {
        // Both return how many of that product are in the cart afterwards (0 = removed)
        Task<int> AddToCartAsync(Product product);
        Task<int> RemoveFromCartAsync(Product product);
        Task<List<ShoppingCartItem>> GetShoppingCartItemsAsync();
        Task ClearCartAsync();
        Task<decimal> GetShoppingCartTotalAsync();
        Task<int> GetShoppingCartItemCountAsync();

        // ProductId -> amount in this cart, e.g. { 7: 2, 20: 1 }. Loaded once per request,
        // so a page with 24 product cards still needs only one query.
        Task<Dictionary<int, int>> GetProductAmountsAsync();
        List<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}

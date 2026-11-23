using eShopEnhanced.Models;
namespace eShopEnhanced.Services
{
    public interface IShoppingCart
    {
        Task AddToCartAsync(Product product);
        Task<int> RemoveFromCartAsync(Product product);
        Task<List<ShoppingCartItem>> GetShoppingCartItemsAsync();
        Task ClearCartAsync();
        Task<decimal> GetShoppingCartTotalAsync();
        Task<int> GetShoppingCartItemCountAsync();
        List<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}

using eShopEnhanced.Models;

namespace eShopEnhanced.Repositories
{
    public interface IOrderRepository
    {
        // Saves the order together with one OrderDetail per item in the visitor's cart
        Task CreateOrderAsync(Order order);
    }
}

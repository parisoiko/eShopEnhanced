using eShopEnhanced.Data;
using eShopEnhanced.Models;
using eShopEnhanced.Services;

namespace eShopEnhanced.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;
        private readonly IShoppingCart _shoppingCart;

        public OrderRepository(eShopEnhancedDbContext eShopEnhancedDbContext, IShoppingCart shoppingCart)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
            _shoppingCart = shoppingCart;
        }

        public async Task CreateOrderAsync(Order order)
        {
            order.OrderPlaced = DateTime.Now;

            List<ShoppingCartItem> shoppingCartItems = await _shoppingCart.GetShoppingCartItemsAsync();
            order.OrderTotal = await _shoppingCart.GetShoppingCartTotalAsync();

            order.OrderDetails = new List<OrderDetail>();

            foreach (ShoppingCartItem shoppingCartItem in shoppingCartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    // Set the foreign key, not the Product itself (same reason as in the cart)
                    ProductId = shoppingCartItem.ProductId,
                    Amount = shoppingCartItem.Amount,
                    // Copy today's price, so later price changes don't change this order
                    Price = shoppingCartItem.Product.Price
                });
            }

            // Adding the order also adds its OrderDetails (EF follows the list)
            _eShopEnhancedDbContext.Orders.Add(order);
            await _eShopEnhancedDbContext.SaveChangesAsync();
        }
    }
}

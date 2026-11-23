using eShopEnhanced.Data;
using eShopEnhanced.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopEnhanced.Services
{
    public class ShoppingCart : IShoppingCart
    {
        private readonly eShopEnhancedDbContext _eShopEnhancedDbContext;

        public string? ShoppingCartId { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = default!;

        private ShoppingCart(eShopEnhancedDbContext eShopEnhancedDbContext)
        {
            _eShopEnhancedDbContext = eShopEnhancedDbContext;
        }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession? session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Session;

            eShopEnhancedDbContext context = services.GetService<eShopEnhancedDbContext>() ?? throw new Exception("Error initializing");

            string cartId = session?.GetString("CartId") ?? Guid.NewGuid().ToString();

            session?.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public async Task AddToCartAsync(Product product)
        {
            var shoppingCartItem =
                    await _eShopEnhancedDbContext.ShoppingCartItems.SingleOrDefaultAsync(
                        s => s.ProductId == product.ProductId && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    // Set the foreign key, not "Product = product": our products are loaded with
                    // AsNoTracking, so EF would think the product is new and try to insert it again.
                    ProductId = product.ProductId,
                    Amount = 1
                };

                _eShopEnhancedDbContext.ShoppingCartItems.Add(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Amount++;
            }
            await _eShopEnhancedDbContext.SaveChangesAsync();
        }

        public async Task<int> RemoveFromCartAsync(Product product)
        {
            var shoppingCartItem =
                    await _eShopEnhancedDbContext.ShoppingCartItems.SingleOrDefaultAsync(
                        s => s.ProductId == product.ProductId && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                    localAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _eShopEnhancedDbContext.ShoppingCartItems.Remove(shoppingCartItem);
                }
            }

            await _eShopEnhancedDbContext.SaveChangesAsync();

            return localAmount;
        }

        public async Task<List<ShoppingCartItem>> GetShoppingCartItemsAsync()
        {
            return ShoppingCartItems ??=
                       await _eShopEnhancedDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                           .Include(s => s.Product)
                           .ToListAsync();
        }

        public async Task ClearCartAsync()
        {
            var cartItems = _eShopEnhancedDbContext
                .ShoppingCartItems
                .Where(cart => cart.ShoppingCartId == ShoppingCartId);

            _eShopEnhancedDbContext.ShoppingCartItems.RemoveRange(cartItems);

            await _eShopEnhancedDbContext.SaveChangesAsync();
        }

        // Total number of pieces in the cart (2 cakes + 1 croissant = 3), for the navbar badge.
        // One small SQL SUM instead of loading every item and its product.
        public async Task<int> GetShoppingCartItemCountAsync()
        {
            return await _eShopEnhancedDbContext.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .SumAsync(c => c.Amount);
        }

        public async Task<decimal> GetShoppingCartTotalAsync()
        {
            var total = await _eShopEnhancedDbContext.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                .Select(c => c.Product.Price * c.Amount).SumAsync();
            return total;
        }
    }
}

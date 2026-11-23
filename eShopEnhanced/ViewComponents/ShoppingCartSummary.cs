using eShopEnhanced.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.ViewComponents
{
    // The "Cart (3)" link in the navbar. Like CategoryMenu, it fetches its own data,
    // so it works on every page without the controllers having to pass anything.
    // Used in the layout as: <vc:shopping-cart-summary />
    public class ShoppingCartSummary : ViewComponent
    {
        private readonly IShoppingCart _shoppingCart;

        public ShoppingCartSummary(IShoppingCart shoppingCart)
        {
            _shoppingCart = shoppingCart;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int itemCount = await _shoppingCart.GetShoppingCartItemCountAsync();

            // Renders Views/Shared/Components/ShoppingCartSummary/Default.cshtml
            return View(itemCount);
        }
    }
}

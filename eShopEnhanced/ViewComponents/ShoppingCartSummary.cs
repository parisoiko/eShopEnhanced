using eShopEnhanced.Services;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.ViewComponents
{
    // Navbar "Cart (3) · 53,10 €" link (<vc:shopping-cart-summary /> in _Layout)
    public class ShoppingCartSummary : ViewComponent
    {
        private readonly IShoppingCart _shoppingCart;

        public ShoppingCartSummary(IShoppingCart shoppingCart)
        {
            _shoppingCart = shoppingCart;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var summary = new ShoppingCartSummaryViewModel(
                await _shoppingCart.GetShoppingCartItemCountAsync(),
                await _shoppingCart.GetShoppingCartTotalAsync());

            return View(summary);
        }
    }
}

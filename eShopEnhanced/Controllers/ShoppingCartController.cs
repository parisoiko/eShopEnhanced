using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.Services;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCart _shoppingCart;

        public ShoppingCartController(IProductRepository productRepository, IShoppingCart shoppingCart)
        {
            _productRepository = productRepository;
            _shoppingCart = shoppingCart;
        }

        public async Task<ViewResult> Index()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            var shoppingCartViewModel = new ShoppingCartViewModel(_shoppingCart, await _shoppingCart.GetShoppingCartTotalAsync());

            return View(shoppingCartViewModel);
        }

        // Cart contents as an HTML fragment for the slide-out panel, fetched by site.js when it opens.
        // NoStore: the browser must never show a stale copy of the cart.
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<PartialViewResult> MiniCart()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            var shoppingCartViewModel = new ShoppingCartViewModel(_shoppingCart, await _shoppingCart.GetShoppingCartTotalAsync());

            return PartialView("_MiniCart", shoppingCartViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToShoppingCart(int productId, string? returnUrl)
        {
            Product? selectedProduct = await _productRepository.GetProductByIdAsync(productId);
            int amount = 0;

            if (selectedProduct != null)
            {
                amount = await _shoppingCart.AddToCartAsync(selectedProduct);
            }

            // fetch() from site.js: answer with JSON so the page can update without a reload
            if (IsJavaScriptRequest())
            {
                return await CartJsonAsync(productId, amount,
                    selectedProduct != null ? $"{selectedProduct.Name} was added to your cart." : null);
            }

            // Back to the originating page with a confirmation toast.
            // Url.IsLocalUrl prevents open redirects through a crafted returnUrl.
            if (selectedProduct != null && !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                TempData["CartMessage"] = $"{selectedProduct.Name} was added to your cart.";
                return LocalRedirect(returnUrl);
            }

            // No returnUrl (the + button on the cart page) -> stay on the cart page
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromShoppingCart(int productId, string? returnUrl)
        {
            Product? selectedProduct = await _productRepository.GetProductByIdAsync(productId);
            int amount = 0;

            if (selectedProduct != null)
            {
                amount = await _shoppingCart.RemoveFromCartAsync(selectedProduct);
            }

            if (IsJavaScriptRequest())
            {
                return await CartJsonAsync(productId, amount, message: null);
            }

            // The - button on a product card (without JavaScript) -> back to that page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // The - button on the cart page -> stay on the cart page
            return RedirectToAction("Index");
        }

        // site.js sends this header with its fetch requests; a normal form post doesn't
        private bool IsJavaScriptRequest() =>
            Request.Headers.XRequestedWith == "XMLHttpRequest";

        // { productId: 7, amount: 2, cartCount: 5, cartTotal: 53.1, cartTotalText: "53,10 €", message: "..." }
        // cartTotalText is formatted server-side with the site culture (el-GR) so it matches the rest of the UI.
        private async Task<JsonResult> CartJsonAsync(int productId, int amount, string? message)
        {
            decimal total = await _shoppingCart.GetShoppingCartTotalAsync();

            return Json(new
            {
                productId,
                amount,
                cartCount = await _shoppingCart.GetShoppingCartItemCountAsync(),
                cartTotal = total,
                cartTotalText = total.ToString("c"),
                message
            });
        }
    }
}

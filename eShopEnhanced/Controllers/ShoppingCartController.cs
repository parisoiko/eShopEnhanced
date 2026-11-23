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

        // /ShoppingCart -> the cart page
        public async Task<ViewResult> Index()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            var shoppingCartViewModel = new ShoppingCartViewModel(_shoppingCart, await _shoppingCart.GetShoppingCartTotalAsync());

            return View(shoppingCartViewModel);
        }

        // /ShoppingCart/MiniCart -> just the cart contents as an HTML fragment (no layout),
        // fetched by JavaScript when the slide-out cart panel opens. See site.js.
        // NoStore: the browser must never show an old copy of the cart.
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<PartialViewResult> MiniCart()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            var shoppingCartViewModel = new ShoppingCartViewModel(_shoppingCart, await _shoppingCart.GetShoppingCartTotalAsync());

            // PartialView: renders Views/ShoppingCart/_MiniCart.cshtml WITHOUT _Layout
            return PartialView("_MiniCart", shoppingCartViewModel);
        }

        // POST /ShoppingCart/AddToShoppingCart  (form fields: productId, optional returnUrl)
        // [HttpPost]: only a form submit can change the cart, never a plain link click.
        // [ValidateAntiForgeryToken]: rejects posts that didn't come from one of our own forms.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToShoppingCart(int productId, string? returnUrl)
        {
            Product? selectedProduct = await _productRepository.GetProductByIdAsync(productId);

            if (selectedProduct != null)
            {
                await _shoppingCart.AddToCartAsync(selectedProduct);
            }

            // Came from a product page -> go back there and show a confirmation.
            // Url.IsLocalUrl only allows addresses on our own site, so nobody can craft a form
            // that adds to the cart and then sends the visitor off to another website.
            if (selectedProduct != null && !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                // TempData survives exactly one redirect: it's shown on the next page, then gone
                TempData["CartMessage"] = $"{selectedProduct.Name} was added to your cart.";
                return LocalRedirect(returnUrl);
            }

            // No returnUrl (the + button on the cart page) -> stay on the cart page
            return RedirectToAction("Index");
        }

        // POST /ShoppingCart/RemoveFromShoppingCart  (form field: productId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectToActionResult> RemoveFromShoppingCart(int productId)
        {
            Product? selectedProduct = await _productRepository.GetProductByIdAsync(productId);

            if (selectedProduct != null)
            {
                await _shoppingCart.RemoveFromCartAsync(selectedProduct);
            }
            return RedirectToAction("Index");
        }
    }
}

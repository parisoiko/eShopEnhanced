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
            int amount = 0;

            if (selectedProduct != null)
            {
                amount = await _shoppingCart.AddToCartAsync(selectedProduct);
            }

            // Sent by site.js (no page reload): answer with JSON so the page can update itself
            if (IsJavaScriptRequest())
            {
                return await CartJsonAsync(productId, amount,
                    selectedProduct != null ? $"{selectedProduct.Name} was added to your cart." : null);
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

        // POST /ShoppingCart/RemoveFromShoppingCart  (form fields: productId, optional returnUrl)
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

        // What site.js needs to update the page:
        //   { productId: 7, amount: 2, cartCount: 5, cartTotal: 53.1, cartTotalText: "53,10 €", message: "..." }
        // cartTotalText is formatted HERE, with the site's culture (el-GR), so the navbar shows
        // exactly the same format as the rest of the site; JavaScript just displays it.
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

using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    // [Authorize] on the controller = every action needs a logged-in user.
    // A visitor who isn't logged in is sent to /Identity/Account/Login?ReturnUrl=/Order/Checkout
    // and comes straight back here after logging in. The cart is kept (it's tied to the session).
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCart _shoppingCart;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderRepository orderRepository, IShoppingCart shoppingCart, UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _shoppingCart = shoppingCart;
            _userManager = userManager;
        }

        // GET /Order/Checkout -> the checkout form, already filled in from the customer's account
        // (they can still change anything, e.g. to send the order to another address)
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View();
            }

            var prefilled = new Order
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                ZipCode = user.ZipCode,
                City = user.City,
                State = user.State,
                Country = user.Country
            };
            return View(prefilled);
        }

        // POST /Order/Checkout -> the filled-in form arrives here.
        // MODEL BINDING: ASP.NET Core creates an Order and fills each property from the form field
        // with the same name (FirstName=Maria -> order.FirstName = "Maria"). While doing that it also
        // checks the rules (required fields...) and writes any problems into ModelState.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            List<ShoppingCartItem> items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            // Our own rule on top of the field rules: no order without products.
            // "" as the key = an error for the whole form, shown by <div asp-validation-summary="ModelOnly">
            if (items.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty. Add some products before checking out.");
            }

            if (ModelState.IsValid)
            {
                await _orderRepository.CreateOrderAsync(order);
                await _shoppingCart.ClearCartAsync();

                // Redirect instead of showing a page directly ("Post/Redirect/Get"):
                // refreshing the thank-you page then can't submit the order a second time.
                // TempData carries the order number across that one redirect.
                TempData["OrderId"] = order.OrderId;
                return RedirectToAction(nameof(CheckoutComplete));
            }

            // Something is wrong: show the form again WITH the order, so everything the visitor
            // typed stays filled in and the error messages appear next to the fields.
            return View(order);
        }

        // GET /Order/CheckoutComplete -> "thank you" page
        [HttpGet]
        public IActionResult CheckoutComplete()
        {
            // The order number arrives through TempData from the Checkout POST (only once)
            object? orderId = TempData["OrderId"];

            // ViewBag: a quick way to hand a single value to the view without a view model
            ViewBag.CheckoutCompleteMessage = orderId != null
                ? $"Thanks for your order #{orderId}! You'll soon be enjoying our freshly baked treats."
                : "Thanks for your order! You'll soon be enjoying our freshly baked treats.";

            return View();
        }
    }
}

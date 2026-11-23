using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    // Anonymous visitors are sent to the login page and returned here afterwards.
    // The cart survives the round trip because it is keyed by the session, not the user.
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

        // Pre-filled from the customer's account; everything stays editable (e.g. to ship elsewhere).
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            List<ShoppingCartItem> items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            // No order without products (form-level error, shown in the validation summary)
            if (items.Count == 0)
            {
                ModelState.AddModelError("", "Your cart is empty. Add some products before checking out.");
            }

            if (ModelState.IsValid)
            {
                await _orderRepository.CreateOrderAsync(order);
                await _shoppingCart.ClearCartAsync();

                // Post/Redirect/Get: refreshing the confirmation page can't resubmit the order.
                TempData["OrderId"] = order.OrderId;
                return RedirectToAction(nameof(CheckoutComplete));
            }

            // Redisplay the form with the user's input and the validation errors
            return View(order);
        }

        [HttpGet]
        public IActionResult CheckoutComplete()
        {
            object? orderId = TempData["OrderId"];

            ViewBag.CheckoutCompleteMessage = orderId != null
                ? $"Thanks for your order #{orderId}! You'll soon be enjoying our freshly baked treats."
                : "Thanks for your order! You'll soon be enjoying our freshly baked treats.";

            return View();
        }
    }
}

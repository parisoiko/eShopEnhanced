using System.Security.Claims;
using eShopEnhanced.Controllers;
using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.Services;
using eShopEnhanced.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace eShopEnhanced.Tests.Controllers
{
    // Unit tests for OrderController. The repository, the cart and the UserManager are all fakes,
    // so we can check WHAT the controller does (which methods it calls, what it returns)
    // without a database or a logged-in browser.
    public class OrderControllerTests
    {
        private readonly Mock<IOrderRepository> _orderRepository = new();
        private readonly Mock<IShoppingCart> _shoppingCart = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManager = CreateUserManagerMock();

        // UserManager is a class, not an interface. Moq can still fake it because the methods we use
        // are virtual; it only needs a (fake) IUserStore for the constructor. The other constructor
        // arguments (password hasher, validators...) aren't used by these tests, so null is fine.
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private OrderController CreateController()
        {
            // A logged-in user, as the [Authorize] attribute would guarantee in the real app
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "user-1"), new Claim(ClaimTypes.Name, "maria@example.com") },
                authenticationType: "Test"));
            var httpContext = new DefaultHttpContext { User = user };

            return new OrderController(_orderRepository.Object, _shoppingCart.Object, _userManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                // TempData needs a provider; a mock that stores nothing is enough here
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        private static Order CreateValidOrder() => new()
        {
            FirstName = "Maria",
            LastName = "Papadopoulou",
            AddressLine1 = "Ermou 10",
            ZipCode = "10563",
            City = "Athens",
            Country = "Greece",
            PhoneNumber = "+30 210 1234567",
            Email = "maria@example.com"
        };

        private void CartContains(params ShoppingCartItem[] items) =>
            _shoppingCart.Setup(c => c.GetShoppingCartItemsAsync()).ReturnsAsync(items.ToList());

        private static ShoppingCartItem CartItem(int amount = 1) => new()
        {
            ShoppingCartItemId = 1,
            ProductId = 7,
            Product = RepositoryMocks.Products.Single(p => p.ProductId == 7),
            Amount = amount,
            ShoppingCartId = "cart-1"
        };

        // ---------- POST Checkout ----------

        [Fact]
        public async Task Checkout_Post_WithEmptyCart_ReturnsViewWithModelErrorAndDoesNotCreateOrder()
        {
            // Arrange
            CartContains();   // nothing in the cart
            var controller = CreateController();
            var order = CreateValidOrder();

            // Act
            var result = await controller.Checkout(order);

            // Assert: the form again, with an error for the whole form (key "")
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(order, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
            Assert.Contains("empty", controller.ModelState[string.Empty]!.Errors.Single().ErrorMessage);

            _orderRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
            _shoppingCart.Verify(c => c.ClearCartAsync(), Times.Never);
        }

        [Fact]
        public async Task Checkout_Post_WithInvalidModelState_ReturnsViewWithSameOrder()
        {
            // Arrange: items in the cart, but model binding found a problem with a field
            CartContains(CartItem());
            var controller = CreateController();
            controller.ModelState.AddModelError(nameof(Order.Email), "This doesn't look like a valid email address");
            var order = CreateValidOrder();
            order.Email = "not-an-email";

            // Act
            var result = await controller.Checkout(order);

            // Assert: the same order object goes back to the view, so the typed values stay filled in
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);   // the default view (Checkout)
            Assert.Same(order, viewResult.Model);
            _orderRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
            _shoppingCart.Verify(c => c.ClearCartAsync(), Times.Never);
        }

        [Fact]
        public async Task Checkout_Post_WithValidOrderAndItems_CreatesOrderClearsCartAndRedirects()
        {
            // Arrange
            CartContains(CartItem(amount: 2));
            _orderRepository.Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .Callback((Order o) => o.OrderId = 42)   // the database would give the order its id
                .Returns(Task.CompletedTask);
            var controller = CreateController();
            var order = CreateValidOrder();

            // Act
            var result = await controller.Checkout(order);

            // Assert: saved once, cart emptied once, then Post/Redirect/Get to the thank-you page
            _orderRepository.Verify(r => r.CreateOrderAsync(order), Times.Once);
            _shoppingCart.Verify(c => c.ClearCartAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.CheckoutComplete), redirect.ActionName);
            Assert.Null(redirect.ControllerName);   // same controller
            Assert.Equal(42, controller.TempData["OrderId"]);
        }

        // ---------- GET Checkout ----------

        [Fact]
        public async Task Checkout_Get_WithLoggedInUser_PrefillsOrderFromAccount()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "Maria",
                LastName = "Papadopoulou",
                Email = "maria@example.com",
                PhoneNumber = "+30 210 1234567",
                AddressLine1 = "Ermou 10",
                AddressLine2 = "3rd floor",
                ZipCode = "10563",
                City = "Athens",
                State = "Attica",
                Country = "Greece"
            };
            _userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var controller = CreateController();

            // Act
            var result = await controller.Checkout();

            // Assert
            var order = Assert.IsType<Order>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("Maria", order.FirstName);
            Assert.Equal("Papadopoulou", order.LastName);
            Assert.Equal("maria@example.com", order.Email);
            Assert.Equal("+30 210 1234567", order.PhoneNumber);
            Assert.Equal("Ermou 10", order.AddressLine1);
            Assert.Equal("3rd floor", order.AddressLine2);
            Assert.Equal("10563", order.ZipCode);
            Assert.Equal("Athens", order.City);
            Assert.Equal("Attica", order.State);
            Assert.Equal("Greece", order.Country);
            Assert.Equal(0, order.OrderId);   // a new order, not saved yet
        }

        [Fact]
        public async Task Checkout_Get_WhenUserNotFound_ReturnsEmptyForm()
        {
            _userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);
            var controller = CreateController();

            var result = await controller.Checkout();

            Assert.Null(Assert.IsType<ViewResult>(result).Model);
        }

        // ---------- CheckoutComplete ----------

        [Fact]
        public void CheckoutComplete_WithOrderIdInTempData_ShowsOrderNumber()
        {
            var controller = CreateController();
            controller.TempData["OrderId"] = 42;

            var result = controller.CheckoutComplete();

            Assert.IsType<ViewResult>(result);
            Assert.Contains("#42", (string)controller.ViewBag.CheckoutCompleteMessage);
        }
    }
}

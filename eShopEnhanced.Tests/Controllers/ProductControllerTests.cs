using eShopEnhanced.Controllers;
using eShopEnhanced.Models;
using eShopEnhanced.Tests.Mocks;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Tests.Controllers
{
    // Unit tests for ProductController. Each test follows Arrange / Act / Assert:
    //   Arrange: create the controller with fake repositories (see Mocks/RepositoryMocks.cs)
    //   Act:     call one action, like a request would
    //   Assert:  check what the action returned
    public class ProductControllerTests
    {
        private static ProductController CreateController() =>
            new ProductController(
                RepositoryMocks.GetProductRepository().Object,   // .Object = the fake object itself
                RepositoryMocks.GetCategoryRepository().Object);

        // ---------- List ----------

        [Fact]
        public async Task List_WithoutCategory_ShowsAllProducts()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.List(null);

            // Assert: a view, whose model is a ProductListViewModel with all 4 products
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ProductListViewModel>(viewResult.Model);
            Assert.Equal(4, model.Products.Count());
            Assert.Equal("All products", model.CurrentCategory);
        }

        [Fact]
        public async Task List_WithCategory_ShowsOnlyThatCategory()
        {
            var controller = CreateController();

            var result = await controller.List("Cakes");

            var model = Assert.IsAssignableFrom<ProductListViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Single(model.Products);
            Assert.Equal("Black Forest Cake", model.Products.Single().Name);
            Assert.Equal("Cakes", model.CurrentCategory);
        }

        [Fact]
        public async Task List_WithLowercaseCategoryFromUrl_UsesTheRealCategoryName()
        {
            // URLs use slugs like /products/pies; the heading should still say "Pies"
            var controller = CreateController();

            var result = await controller.List("pies");

            var model = Assert.IsAssignableFrom<ProductListViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(3, model.Products.Count());
            Assert.Equal("Pies", model.CurrentCategory);
        }

        [Fact]
        public async Task List_WithUnknownCategory_ReturnsNotFound()
        {
            var controller = CreateController();

            var result = await controller.List("pizza");

            Assert.IsType<NotFoundResult>(result);
        }

        // ---------- Details ----------

        [Fact]
        public async Task Details_WithCorrectSlug_ShowsTheProduct()
        {
            var controller = CreateController();

            var result = await controller.Details(7, "black-forest-cake");

            var product = Assert.IsType<Product>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(7, product.ProductId);
            Assert.Equal("Black Forest Cake", product.Name);
        }

        [Fact]
        public async Task Details_WithUnknownId_ReturnsNotFound()
        {
            var controller = CreateController();

            var result = await controller.Details(999, null);

            Assert.IsType<NotFoundResult>(result);
        }

        // [Theory] = the same test run once per [InlineData] line
        [Theory]
        [InlineData(null)]              // /products/7
        [InlineData("old-name")]        // /products/7/old-name
        [InlineData("Black-Forest-Cake")] // wrong capitals still count as wrong
        public async Task Details_WithMissingOrWrongSlug_RedirectsPermanentlyToTheCorrectUrl(string? slug)
        {
            var controller = CreateController();

            var result = await controller.Details(7, slug);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirect.Permanent);                       // 301, so search engines update their links
            Assert.Equal(nameof(ProductController.Details), redirect.ActionName);
            Assert.Equal(7, redirect.RouteValues!["id"]);
            Assert.Equal("black-forest-cake", redirect.RouteValues["slug"]);
        }
    }
}

using eShopEnhanced.Helpers;
using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    // Attribute routing: every URL of this controller starts with /products
    [Route("products")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // /products            -> every product
        // /products/cupcakes   -> only that category's products
        [HttpGet("")]
        [HttpGet("{category}")]
        public async Task<IActionResult> List(string? category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return View(new ProductListViewModel(await _productRepository.GetAllProductsAsync(), "All products"));
            }

            // Unknown category in the URL (e.g. /products/pizza) -> 404 page
            Category? selectedCategory = await _categoryRepository.GetCategoryByNameAsync(category);

            if (selectedCategory == null)
            {
                return NotFound();
            }

            ProductListViewModel productListViewModel = new ProductListViewModel(
                await _productRepository.GetProductsByCategoryAsync(selectedCategory.CategoryName),
                selectedCategory.CategoryName);

            return View(productListViewModel);
        }

        // /products/7/black-forest-cake
        // {id:int} only matches numbers, so /products/7 is a product and /products/cakes is a category.
        // The slug is only there to make the URL readable; the id alone finds the product.
        [HttpGet("{id:int}/{slug?}")]
        public async Task<IActionResult> Details(int id, string? slug)
        {
            Product? product = await _productRepository.GetProductByIdAsync(id);

            // No product with that id (e.g. /products/999) -> 404 page
            if (product == null)
            {
                return NotFound();
            }

            // Missing or wrong slug (/products/7 or /products/7/old-name) -> permanent redirect
            // to the one correct URL, so every product has exactly one address
            string correctSlug = Slug.From(product.Name);
            if (slug != correctSlug)
            {
                return RedirectToActionPermanent(nameof(Details), new { id, slug = correctSlug });
            }

            return View(product);
        }
    }
}

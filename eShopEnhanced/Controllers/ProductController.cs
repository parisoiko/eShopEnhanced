using eShopEnhanced.Helpers;
using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
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

        // /products/search?q=cake: hosts the Blazor live search; q comes from the navbar search box.
        // The literal "search" segment takes precedence over the {category} route below.
        [HttpGet("search")]
        public IActionResult Search(string? q)
        {
            ViewBag.SearchQuery = q ?? string.Empty;
            return View();
        }

        // HTML fragment for the navbar search dropdown, fetched by site.js while the visitor types
        [HttpGet("search/suggestions")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SearchSuggestions(string? q, string? returnUrl)
        {
            // Return target for the quick-add buttons; local URLs only (open-redirect protection)
            ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            string query = (q ?? string.Empty).Trim();
            List<Product> products = query.Length >= 2
                ? await _productRepository.SearchProductsAsync(query)
                : new List<Product>();

            ViewBag.SearchQuery = query;
            ViewBag.TotalFound = products.Count;

            // Only the first 6 in the dropdown; "See all results" opens the full search page
            return PartialView("_SearchSuggestions", products.Take(6).ToList());
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
        // {id:int} keeps product ids apart from category names; the slug is cosmetic, the id finds the product.
        [HttpGet("{id:int}/{slug?}")]
        public async Task<IActionResult> Details(int id, string? slug)
        {
            Product? product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Missing or outdated slug: 301 to the canonical URL, so each product has exactly one address
            string correctSlug = Slug.From(product.Name);
            if (slug != correctSlug)
            {
                return RedirectToActionPermanent(nameof(Details), new { id, slug = correctSlug });
            }

            return View(product);
        }
    }
}

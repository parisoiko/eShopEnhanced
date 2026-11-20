using eShopEnhanced.Models;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    public class ProductController: Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // /Product/List                     -> every product
        // /Product/List?category=Cupcakes   -> only that category's products
        public IActionResult List(string? category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return View(new ProductListViewModel(_productRepository.AllProducts, "All products"));
            }

            // Unknown category in the URL (e.g. ?category=Pizza) -> 404 page
            Category? selectedCategory = _categoryRepository.AllCategories
                .FirstOrDefault(c => c.CategoryName == category);

            if (selectedCategory == null)
            {
                return NotFound();
            }

            ProductListViewModel productListViewModel = new ProductListViewModel(
                _productRepository.ProductsByCategory(selectedCategory.CategoryName),
                selectedCategory.CategoryName);

            return View(productListViewModel);
        }

        // /Product/Details/3 -> the "3" comes from the {id?} part of the default route
        public IActionResult Details(int id)
        {
            Product? product = _productRepository.GetProductById(id);

            // No product with that id (e.g. /Product/Details/999) -> 404 page
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult ListCategories()
        {
            return View(_categoryRepository.AllCategories);
        }
    }
}
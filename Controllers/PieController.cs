using eShopEnhanced.Models;
using eShopEnhanced.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.Controllers
{
    public class PieController: Controller
    {
        private readonly IPieRepository _pieRepository;
        private readonly ICategoryRepository _categoryRepository;
        
        public PieController(IPieRepository pieRepository, ICategoryRepository categoryRepository)
        {
            _pieRepository = pieRepository;
            _categoryRepository = categoryRepository;
        }

        // /Pie/List                     -> every product
        // /Pie/List?category=Cupcakes   -> only that category's products
        public IActionResult List(string? category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return View(new PieListViewModel(_pieRepository.AllPies, "All products"));
            }

            // Unknown category in the URL (e.g. ?category=Pizza) -> 404 page
            Category? selectedCategory = _categoryRepository.AllCategories
                .FirstOrDefault(c => c.CategoryName == category);

            if (selectedCategory == null)
            {
                return NotFound();
            }

            PieListViewModel pieListViewModel = new PieListViewModel(
                _pieRepository.PiesByCategory(selectedCategory.CategoryName),
                selectedCategory.CategoryName);

            return View(pieListViewModel);
        }

        public IActionResult ListCategories()
        {
            return View(_categoryRepository.AllCategories);
        }
    }
}
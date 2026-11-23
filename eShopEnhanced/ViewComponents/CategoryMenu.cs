using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.ViewComponents
{
    // Navbar "Products" dropdown (<vc:category-menu /> in _Layout)
    public class CategoryMenu : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryMenu(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IEnumerable<Category> categories = await _categoryRepository.GetAllCategoriesAsync();

            return View(categories);
        }
    }
}

using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace eShopEnhanced.ViewComponents
{
    // A view component is like a mini controller + view that can be dropped into any page,
    // including _Layout.cshtml, and fetches its own data.
    // Used in the layout as: <vc:category-menu />
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

            // Renders Views/Shared/Components/CategoryMenu/Default.cshtml
            return View(categories);
        }
    }
}

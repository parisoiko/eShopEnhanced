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

        public IViewComponentResult Invoke()
        {
            IEnumerable<Category> categories = _categoryRepository.AllCategories;

            // Renders Views/Shared/Components/CategoryMenu/Default.cshtml
            return View(categories);
        }
    }
}

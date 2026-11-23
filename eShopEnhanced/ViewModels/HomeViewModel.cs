using eShopEnhanced.Models;

namespace eShopEnhanced.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> PicksOfTheWeek { get; }
        public IEnumerable<Category> Categories { get; }

        public HomeViewModel(IEnumerable<Product> picksOfTheWeek, IEnumerable<Category> categories)
        {
            PicksOfTheWeek = picksOfTheWeek;
            Categories = categories;
        }
    }
}

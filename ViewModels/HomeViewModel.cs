using eShopEnhanced.Models;

namespace eShopEnhanced.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Pie> PiesOfTheWeek { get; }
        public IEnumerable<Category> Categories { get; }

        public HomeViewModel(IEnumerable<Pie> piesOfTheWeek, IEnumerable<Category> categories)
        {
            PiesOfTheWeek = piesOfTheWeek;
            Categories = categories;
        }
    }
}

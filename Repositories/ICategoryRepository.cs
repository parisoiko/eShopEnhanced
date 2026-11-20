using eShopEnhanced.Models;
namespace eShopEnhanced.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> AllCategories {get;}
    }
}
using eShopEnhanced.Models;
namespace eShopEnhanced.Repositories
{
    public interface ISubCategoryRepository
    {
        IEnumerable<SubCategory> AllSubCategories { get; }
    }
}

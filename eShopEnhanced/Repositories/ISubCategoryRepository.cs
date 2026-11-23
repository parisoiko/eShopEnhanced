using eShopEnhanced.Models;

namespace eShopEnhanced.Repositories
{
    public interface ISubCategoryRepository
    {
        Task<List<SubCategory>> GetAllSubCategoriesAsync();
    }
}

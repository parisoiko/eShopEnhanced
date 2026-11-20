using eShopEnhanced.Models;

namespace eShopEnhanced.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();

        // Case-insensitive: "cupcakes" finds "Cupcakes". Returns null if there's no such category.
        Task<Category?> GetCategoryByNameAsync(string categoryName);
    }
}

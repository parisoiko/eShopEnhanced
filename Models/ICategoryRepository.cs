namespace eShopEnhanced.Models
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> AllCategories {get;}
    }
}
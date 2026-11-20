namespace eShopEnhanced.Models
{
    public interface ISubCategoryRepository
    {
        IEnumerable<SubCategory> AllSubCategories { get; }
    }
}

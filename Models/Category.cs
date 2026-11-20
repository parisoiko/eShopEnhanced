namespace eShopEnhanced.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // e.g. "Pies" has "Fruit pies" and "Seasonal pies"
        public List<SubCategory>? SubCategories { get; set; }
    }
}

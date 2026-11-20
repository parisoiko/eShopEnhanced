namespace eShopEnhanced.Models
{
    public class SubCategory
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // The top-level category this belongs to (e.g. "Fruit pies" belongs to "Pies")
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public List<Pie>? Pies { get; set; }
    }
}

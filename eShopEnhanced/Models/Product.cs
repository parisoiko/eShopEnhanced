namespace eShopEnhanced.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? AllergyInformation { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public bool IsPickOfTheWeek { get; set; }
        public bool InStock { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = default!;
    }
}

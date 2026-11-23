namespace eShopEnhanced.Models
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }

        // The foreign key, written out so we can set it directly. It maps to the ProductId
        // column the migration already created (EF had made it as a hidden "shadow" property).
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public int Amount { get; set; }
        public string? ShoppingCartId { get; set; }
        
    }
}
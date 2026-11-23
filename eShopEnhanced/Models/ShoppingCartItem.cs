namespace eShopEnhanced.Models
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }

        // Explicit FK so it can be set without attaching a Product (see ShoppingCart.AddToCartAsync).
        // Maps to the existing ProductId column, previously a shadow property.
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public int Amount { get; set; }
        public string? ShoppingCartId { get; set; }
        
    }
}
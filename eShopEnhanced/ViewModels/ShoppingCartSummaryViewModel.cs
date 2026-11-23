namespace eShopEnhanced.ViewModels
{
    // What the navbar cart link shows: "Cart (3) · 53,10 €"
    public record ShoppingCartSummaryViewModel(int ItemCount, decimal Total);
}

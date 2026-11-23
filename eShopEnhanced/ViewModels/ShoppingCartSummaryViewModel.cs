namespace eShopEnhanced.ViewModels
{
    // What the navbar cart link shows: "Cart (3) · 53,10 €".
    // A "record" is a short way to write a small class that just holds values.
    public record ShoppingCartSummaryViewModel(int ItemCount, decimal Total);
}

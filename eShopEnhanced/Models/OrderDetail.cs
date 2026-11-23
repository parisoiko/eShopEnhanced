namespace eShopEnhanced.Models
{
    // One line of an order: which product, how many, and the price at the moment of ordering
    // (copied, so later price changes don't change old orders).
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}
